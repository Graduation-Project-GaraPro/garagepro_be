using System.Text.Json;
using System.Text;
using BusinessObject.PayOsModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.PayOsClients;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using Dtos.PayOsDtos;
using Services.PaymentServices;
using System.Security.Claims;
using Repositories.WebhookInboxRepositories;
using BussinessObject;
using Services.BillServices;
using Dtos.Bills;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _service;
        private readonly IPayOsClient _payos;
        private readonly IWebhookInboxRepository _webhookInboxRepo;
        private readonly IRepairOrderPaymentService _paymentService;
        public PaymentsController(IPaymentService service, IPayOsClient payos, IWebhookInboxRepository webhookInboxRepo, IRepairOrderPaymentService paymentService)
        {
            _service = service;
            _payos = payos;
            _webhookInboxRepo= webhookInboxRepo;
            _paymentService = paymentService;
        }

        [HttpGet("{repairOrderId:guid}/payment")]
        public async Task<ActionResult<RepairOrderPaymentDto>> GetPaymentInfo(Guid repairOrderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub"); 
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _paymentService.GetRepairOrderPaymentInfoAsync(repairOrderId, userId);

            if (result == null)
                return NotFound(new { message = "RepairOrder không tồn tại." });

            return Ok(result);
        }

        [Authorize]
        [HttpPost("create-link")]
        public async Task<IActionResult> CreateLink([FromBody] CreatePaymentRequest req, CancellationToken ct)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub"); // hoặc tên claim chứa idUser
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var link = await _service.CreatePaymentAndLinkAsync(req, userId, ct);
                return Ok(link);
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] JsonElement body, CancellationToken ct)
        {
            try
            {
                // 1. Validate payload structure
                if (!body.TryGetProperty("data", out var dataEl) ||
                    !body.TryGetProperty("signature", out var sigEl))
                {
                   
                    return BadRequest(new { error = "invalid_payload_structure" });
                }

                var signature = sigEl.GetString();
                if (string.IsNullOrEmpty(signature))
                {
                    
                    return BadRequest(new { error = "missing_signature" });
                }

                // 2. Verify signature
                if (!_payos.VerifyWebhookSignature(dataEl, signature))
                {
                    
                    return Unauthorized(new { error = "invalid_signature" });
                }

                // 3. Extract order code
                long orderCode = dataEl.TryGetProperty("orderCode", out var oc) ? oc.GetInt64() : 0;
                if (orderCode <= 0)
                {
                   
                    return BadRequest(new { error = "invalid_order_code" });
                }

                // 4. Create webhook inbox record
                var webhookInbox = new WebhookInbox
                {
                    Provider = "PayOS",
                    OrderCode = orderCode,
                    Payload = body.ToString(),
                    Signature = signature,
                    Status = WebhookStatus.Pending,
                    Attempts = 0,
                    ReceivedAt = DateTime.UtcNow
                };

                await _webhookInboxRepo.AddAsync(webhookInbox);

                // 5. Save to database - với webhook nên dùng CancellationToken.None
                // để đảm bảo lưu thành công ngay cả khi client disconnect
                await _webhookInboxRepo.SaveChangesAsync(CancellationToken.None);

             

                // 6. Có thể trigger background processing ở đây
                // _ = _backgroundService.ProcessWebhookAsync(webhookInbox.Id);

                return Ok(new
                {
                    received = true,
                    webhookId = webhookInbox.Id,
                    orderCode = orderCode
                });
            }
            catch (Exception ex)
            {
                

                // Vẫn trả về 200 để PayOS không retry liên tục
                return Ok(new
                {
                    received = true,
                    note = "webhook_stored_with_errors"
                });
            }
        }
        [HttpGet("cancel")]
        public async Task<IActionResult> Cancel([FromQuery] long orderCode, [FromQuery] string status, [FromQuery] bool cancel = false, CancellationToken ct = default)
        {
            // 1) Đánh dấu hủy ở hệ thống của bạn từ tín hiệu redirect
            var payment = await _service.GetByIdAsync(orderCode);
            if (payment != null && payment.Status != PaymentStatus.Paid)
            {
                payment.Status = PaymentStatus.Cancelled; // hoặc UserCancelled
                payment.UpdatedAt = DateTime.UtcNow;
                await _service.UpdateAsync(payment, ct);                
            }

            // 2) (Tuỳ chọn) Vô hiệu hoá link trực tiếp trên PayOS
            try
            {
                await _payos.CancelPaymentLinkAsync(orderCode, "User cancelled at checkout", ct);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "CancelPaymentLinkAsync failed for orderCode {OrderCode}", orderCode);
                // Không chặn luồng người dùng; bạn có thể hiển thị thông báo nhẹ hoặc thử lại nền.
            }

            // 3) Điều hướng UI phù hợp
            return Redirect($"myapp://payment/cancel?orderCode={orderCode}");

        }


        [HttpGet("status/{orderCode:long}")]
        public async Task<IActionResult> GetStatus(long orderCode, CancellationToken ct)
        {
            var dto = await _service.GetStatusByOrderCodeAsync(orderCode, ct);
            return Ok(dto);
        }

    }
}
