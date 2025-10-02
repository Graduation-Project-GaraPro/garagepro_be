using BusinessObject.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Technician;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobTechnicianController : ControllerBase
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
        public async Task<IActionResult> GetMyJobs()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { Message = "Bạn cần đăng nhập để xem danh sách công việc." });
            }

            var isTechnician = await _userManager.IsInRoleAsync(user, "Technician");
            if (!isTechnician)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { Message = "Bạn không có quyền xem công việc. Chỉ Technician được phép." });
            }

            var jobs = await _technicianService.GetJobsByTechnicianAsync(user.Id);
            if (jobs == null || !jobs.Any())
            {
                return Ok(new { Message = "Hiện tại bạn chưa được gán công việc nào." });
            }

            var result = jobs.Select(job => new
            {
                job.JobId,
                job.JobName,
                job.Status,
                job.Deadline,
                job.TotalAmount,
                job.Note,
                job.CreatedAt,
                job.UpdatedAt,
                job.Level,
                ServiceName = job.Service?.ServiceName,
                RepairOrderId = job.RepairOrderId,
                Parts = job.JobParts?.Select(jp => new
                {
                    jp.PartId,
                    PartName = jp.Part?.Name
                }).ToList(),
                Repairs = job.Repairs?.Select(r => new
                {
                    r.RepairId,
                    r.Description,
                    r.Status,
                    r.Notes
                }).ToList()
            }).ToList();

            return Ok(result);
        }
    }
}