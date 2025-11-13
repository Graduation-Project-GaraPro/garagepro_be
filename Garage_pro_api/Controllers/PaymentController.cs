using Microsoft.AspNetCore.Mvc;
using Services.PaymentServices;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Garage_pro_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Create a payment for a repair order
        /// </summary>
        /// <param name="repairOrderId">The ID of the repair order to pay for</param>
        /// <returns>A URL to redirect the user to the payment gateway</returns>
        [HttpPost("create/{repairOrderId}")]
        public async Task<ActionResult<string>> CreatePayment(Guid repairOrderId)
        {
            try
            {
                // Get the current user ID
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var paymentUrl = await _paymentService.CreatePaymentUrlAsync(repairOrderId, userId);
                return Ok(new { PaymentUrl = paymentUrl });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating payment: {ex.Message}");
            }
        }

        /// <summary>
        /// Process IPN (Instant Payment Notification) from VnPay
        /// </summary>
        /// <returns>Result of IPN processing</returns>
        [HttpPost("ipn")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> IpnAction()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var result = await _paymentService.ProcessIpnAsync(Request.QueryString.Value);
                    return Ok(result);
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(ex.Message);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return NotFound("Không tìm thấy thông tin thanh toán.");
        }

        /// <summary>
        /// Process callback from VnPay after payment attempt
        /// </summary>
        /// <returns>Payment result information</returns>
        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> Callback()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var result = await _paymentService.ProcessCallbackAsync(Request.QueryString.Value);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return NotFound("Không tìm thấy thông tin thanh toán.");
        }
    }
}