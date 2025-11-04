using BusinessObject.Enums;
using BusinessObject.FcmDataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.FCMServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IFcmService _fcmService;

        public NotificationController(IFcmService fcmService)
        {
            _fcmService = fcmService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromQuery] string token, [FromQuery] string title, [FromQuery] string body)
        {
            try
            {
                //await _fcmService.SendNotificationAsync(token, title, body);
                return Ok("Notification sent successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("sendWithData")]
        public async Task<IActionResult> SendNotificationWithData([FromQuery] string token, [FromQuery] string title, [FromQuery] string body)
        {
            try
            {
                var payload = new FcmDataPayload
                {
                    Type = NotificationType.Order,
                    Title = "New Order Received",
                    Body = "Order #123 has been placed successfully!",
                    EntityKey = EntityKeyType.quotationId,
                    EntityId = Guid.Parse("361F1EAE-46FE-4C2E-A725-28CE5CBB3734"),
                    Screen = AppScreen.QuotationDetailFragment
                };

                await _fcmService.SendFcmMessageAsync(token, payload);
                return Ok("Notification sent successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
