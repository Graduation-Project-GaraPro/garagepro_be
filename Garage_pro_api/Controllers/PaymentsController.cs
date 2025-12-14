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
using Microsoft.AspNetCore.Identity;
using BusinessObject;
using BusinessObject.Authentication;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IQrCodeService _qrCodeService;
        private readonly IRepairOrderPaymentService _paymentService;

        public PaymentsController(IPaymentService service, IPayOsClient payos, IWebhookInboxRepository webhookInboxRepo, UserManager<ApplicationUser> userManager, IQrCodeService qrCodeService, IRepairOrderPaymentService paymentService)
        {
            _service = service;
            _payos = payos;
            _webhookInboxRepo = webhookInboxRepo;
            _userManager = userManager;
            _qrCodeService = qrCodeService;
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
        
        /// <summary>
        /// Get payment preview for a repair order (used by manager before cash payment)
        /// </summary>
        [Authorize]
        [HttpGet("preview/{repairOrderId}")]
        public async Task<IActionResult> GetPaymentPreview(Guid repairOrderId, CancellationToken ct = default)
        {
            try
            {
                // Get the current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? User.FindFirstValue("sub");
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Check if user is a manager
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Unauthorized();

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Manager"))
                    return Forbid("Only managers can preview payments for repair orders");

                var preview = await _service.GetPaymentPreviewAsync(repairOrderId, ct);
                return Ok(preview);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Manager creates a payment record for a completed repair order
        /// </summary>
        [Authorize]
        [HttpPost("manager-create/{repairOrderId}")]
        public async Task<IActionResult> ManagerCreatePayment(Guid repairOrderId, [FromBody] ManagerCreatePaymentDto dto, CancellationToken ct = default)
        {
            try
            {
                // Get the current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? User.FindFirstValue("sub");
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Check if user is a manager
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Unauthorized();

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Manager"))
                    return Forbid("Only managers can create payments for repair orders");

                // Validate payment method
                if (dto.Method != PaymentMethod.Cash && dto.Method != PaymentMethod.PayOs)
                    return BadRequest("Invalid payment method. Only Cash and PayOs are supported.");

                // Get the repair order to determine the amount
                var repairOrder = await _service.GetRepairOrderByIdAsync(repairOrderId);
                if (repairOrder == null)
                    return NotFound($"Repair order with ID {repairOrderId} not found");

                // Check if there's already a paid payment
                var existingPayments = await _service.GetByRepairOrderIdAsync(repairOrderId);
                var paidPayment = existingPayments?.FirstOrDefault(p => p.Status == PaymentStatus.Paid);
                if (paidPayment != null) 
                    return BadRequest($"Payment {paidPayment.PaymentId} already paid");

                // Calculate the amount to pay (actual cost from approved quotations)
                var amountToPay = repairOrder.Cost;

                // Create the payment record
                var payment = await _service.CreateManualPaymentAsync(repairOrderId, userId, amountToPay, dto.Method, ct);
                
                // If payment method is PayOs, generate QR code as well
                string? qrCodeData = null;
                if (dto.Method == PaymentMethod.PayOs)
                {
                    qrCodeData = await _qrCodeService.GeneratePaymentQrCodeAsync(repairOrderId, amountToPay, "Payment for repair order");
                }
                
                var response = new { 
                    Message = "Payment record created successfully", 
                    PaymentId = payment.PaymentId,
                    Method = payment.Method,
                    Amount = payment.Amount,
                    Status = payment.Status,
                    QrCodeData = qrCodeData
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PaymentsController] Error creating QR payment: {ex.Message}");
                Console.WriteLine($"[PaymentsController] Stack trace: {ex.StackTrace}");
                return BadRequest(new { Message = ex.Message, Error = ex.ToString() });
            }
        }

        /// <summary>
        /// Manager creates a PayOS QR payment link for a completed repair order
        /// This reuses the PayOS implementation from Android but adapted for manager web
        /// </summary>
        [Authorize]
        [HttpPost("manager-qr-payment/{repairOrderId}")]
        public async Task<IActionResult> ManagerCreateQrPayment(Guid repairOrderId, [FromBody] ManagerCreatePaymentDto dto, CancellationToken ct = default)
        {
            try
            {
                // Get the current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? User.FindFirstValue("sub");
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Check if user is a manager
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Unauthorized();

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Manager"))
                    return Forbid("Only managers can create QR payments for repair orders");

                // Create PayOS payment link using the reusable service
                var result = await _service.CreateManagerPayOsPaymentAsync(repairOrderId, userId, null, ct);

                var response = new
                {
                    Message = "PayOS QR payment link created successfully",
                    PaymentId = result.PaymentId,
                    OrderCode = result.OrderCode,
                    CheckoutUrl = result.CheckoutUrl,
                    QrCodeUrl = result.CheckoutUrl // The checkout URL can be used to generate QR code on frontend
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
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

        /// <summary>
        /// Get payment summary for a repair order
        /// </summary>
        /// <param name="repairOrderId">The ID of the repair order</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Payment summary information</returns>
        [Authorize]
        [HttpGet("summary/{repairOrderId}")]
        public async Task<IActionResult> GetPaymentSummary(Guid repairOrderId, CancellationToken ct = default)
        {
            try
            {
                var summary = await _service.GetPaymentSummaryAsync(repairOrderId, ct);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

    }
}