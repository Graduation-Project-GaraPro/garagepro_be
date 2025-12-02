using System.Security.Claims;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.FcmDataModels;
using BusinessObject.RequestEmergency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Repositories.EmergencyRequestRepositories;
using Services;
using Services.EmergencyRequestService;
using Services.FCMServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
   
    [ApiController]
    public class TechnicianEmergencyController : ControllerBase
    {

        private readonly ITechnicianEmergencyService _technicianEmergencyService;
        private readonly IFcmService _fcmService;
        private readonly IUserService _userService;
        public TechnicianEmergencyController(ITechnicianEmergencyService technicianEmergencyService, IFcmService fcmService, IUserService userService)
        {
            _technicianEmergencyService = technicianEmergencyService;
            _fcmService = fcmService;
            _userService = userService; 
        }


        [Authorize]
        [HttpGet("technician/me")]
        public async Task<IActionResult> GetMyEmergencies()
        {
            var technicianId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(technicianId))
                return Unauthorized();

            var result = await _technicianEmergencyService.GetTechnicianEmergenciesAsync(technicianId);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("location/update")]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateTechnicianLocationRequest request)
        {
            //var technicianId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //if (string.IsNullOrEmpty(technicianId))
            //    return Unauthorized("Technician not authenticated");

            //var success = await _emergencyRequestService.UpdateTechnicianLocationAsync(
            //    technicianId,
            //    request.Latitude,
            //    request.Longitude
            //);

            //if (!success)
            //    return BadRequest("Failed to update location");

            return Ok(new { message = "Location updated successfully" });
        }

        [HttpGet("technician/id")]
        public async Task<IActionResult> GetTechId()
        {
            var technicianId = User.FindFirst("sub")?.Value; // hoặc claim khác bạn đang dùng

            

            return Ok(technicianId);
        }

        [HttpPost("assign-tech")]
        public async Task<IActionResult> AssignTechnician([FromBody] AssignTechRequest request)
        {
            if (string.IsNullOrEmpty(request.TechnicianId))
                return BadRequest("TechnicianId required");

            var success = await _technicianEmergencyService.AssignTechnicianAsync(request.EmergencyId, request.TechnicianId);


            var user = await _userService.GetUserByIdAsync(request.TechnicianId);


            if (user != null && user.DeviceId != null)
            {
                var FcmNotification = new FcmDataPayload
                {
                    Type = NotificationType.Emergency,
                    Title = "Emergency case",
                    Body = "New Emergency case for you",
                    EntityKey = EntityKeyType.repairOrderId,
                    EntityId = Guid.NewGuid(),
                    Screen = AppScreen.ReportsFragment
                };
                await _fcmService.SendFcmMessageAsync(user?.DeviceId, FcmNotification);

               
            }

            if (!success)
                return NotFound("Emergency request not found");

            return Ok("Technician assigned successfully");
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateEmergencyStatus(
            Guid id,
            [FromBody] UpdateEmergencyStatusRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var technicianId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(technicianId))
                return Unauthorized();

            
            var success = await _technicianEmergencyService.UpdateEmergencyStatusAsync(
                id,
                dto.Status,
                dto.RejectReason,
                technicianId
            );

            if (!success)
                return NotFound(new { message = "Không tìm thấy yêu cầu cứu hộ." });

            
            return Ok(new { message = "Cập nhật trạng thái thành công." });
        }
    }
    public class AssignTechRequest
    {
        public Guid EmergencyId { get; set; }
        public string TechnicianId { get; set; }
    }
    public class UpdateTechnicianLocationRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
    public class UpdateEmergencyStatusRequestDto
    {
        public RequestEmergency.EmergencyStatus Status { get; set; }
        public string? RejectReason { get; set; }
       
    }
}
