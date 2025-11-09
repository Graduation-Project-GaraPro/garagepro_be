using BusinessObject.Authentication;
using Dtos.InspectionAndRepair;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.InspectionAndRepair;
namespace Garage_pro_api.Controllers
{
    [Route("odata/[controller]")]
    [ApiController]
    [Authorize(Roles = "Technician")]
    public class JobTechnicianController : ODataController
    {
        private readonly IJobTechnicianService _technicianService;
        private readonly UserManager<ApplicationUser> _userManager;
        public JobTechnicianController(IJobTechnicianService technicianService, UserManager<ApplicationUser> userManager)
        {
            _technicianService = technicianService;
            _userManager = userManager;
        }
        [HttpGet("my-jobs")]
        [Authorize]
        [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
        public async Task<IActionResult> GetMyJobs()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "Bạn cần đăng nhập để xem danh sách công việc." });
            var isTechnician = await _userManager.IsInRoleAsync(user, "Technician");
            if (!isTechnician)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { Message = "Bạn không có quyền xem công việc." });
            var jobDtos = await _technicianService.GetJobsByTechnicianAsync(user.Id);
            if (!jobDtos.Any())
                return Ok(new { Message = "Hiện tại bạn chưa có công việc nào trong tiến trình hoạt động." });
            return Ok(jobDtos.AsQueryable());
        }
        [HttpGet("my-jobs/{jobId}")]
        [Authorize]
        [EnableQuery]
        public async Task<IActionResult> GetJobById(Guid jobId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "Bạn cần đăng nhập." });
            var isTechnician = await _userManager.IsInRoleAsync(user, "Technician");
            if (!isTechnician)
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = "Bạn không có quyền truy cập." });
            var jobDto = await _technicianService.GetJobByIdAsync(user.Id, jobId);
            if (jobDto == null)
                return NotFound(new { Message = "Không tìm thấy công việc hoặc bạn không có quyền truy cập." });
            return Ok(jobDto);
        }
        [HttpPut("update-status")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> UpdateJobStatus([FromBody] JobStatusUpdateDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            try
            {
                var success = await _technicianService.UpdateJobStatusAsync(user.Id, dto);
                if (success)
                    return Ok(new { Message = "Cập nhật trạng thái công việc thành công." });
                return BadRequest(new { Message = "Không thể cập nhật trạng thái công việc." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}