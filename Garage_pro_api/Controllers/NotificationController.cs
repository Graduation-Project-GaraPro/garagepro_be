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
                await _fcmService.SendNotificationAsync(token, title, body);
                return Ok("Notification sent successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
