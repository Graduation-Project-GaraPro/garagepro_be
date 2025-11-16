using BusinessObject.Enums;
using BusinessObject.FcmDataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.FCMServices;
using Services.Notifications;
using System.Security.Claims;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IFcmService _fcmService;
        private readonly INotificationService _notificationService;
        public NotificationController(IFcmService fcmService, INotificationService notificationService)
        {
            _fcmService = fcmService;
            _notificationService = notificationService;
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
        //      
        [HttpGet]
        [Authorize(Roles = "Technician")] 
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("unread")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        [Authorize(Roles = "Technician")] 
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { unreadCount = count });
        }
        
        [HttpPut("{id}/read")]
        [Authorize(Roles = "Technician")] 
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var result = await _notificationService.MarkNotificationAsReadAsync(id, userId);

            if (!result)
                return NotFound("Notification not found or you don't have permission");

            return NoContent();
        }

        [HttpPut("read-all")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            await _notificationService.MarkAllNotificationsAsReadAsync(userId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var result = await _notificationService.DeleteNotificationAsync(id, userId);

            if (!result)
                return NotFound("Notification not found or you don't have permission");

            return NoContent();
        }
    }
}
