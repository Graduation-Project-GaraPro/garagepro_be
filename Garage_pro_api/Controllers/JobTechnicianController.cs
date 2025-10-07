using BusinessObject.Authentication;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.Technician;

namespace Garage_pro_api.Controllers
{
    [Route("odata/[controller]")]
    [ApiController]
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

            // 🔹 Lọc chỉ những job có trạng thái New, InProgress, Completed
            var filteredJobs = jobs
                .Where(j => j.Status == JobStatus.New ||
                            j.Status == JobStatus.InProgress ||
                            j.Status == JobStatus.Completed)
                .ToList();

            if (!filteredJobs.Any())
            {
                return Ok(new { Message = "Hiện tại bạn chưa có công việc nào trong tiến trình hoạt động." });
            }

            var result = filteredJobs.Select(job => new
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
                Vehicle = job.RepairOrder?.Vehicle != null ? new
                {
                    job.RepairOrder.Vehicle.VehicleId,
                    LicensePlate = job.RepairOrder.Vehicle.LicensePlate,
                    VIN = job.RepairOrder.Vehicle.VIN,
                    BrandId = job.RepairOrder.Vehicle.BrandId,
                    ModelId = job.RepairOrder.Vehicle.ModelId,
                    ColorId = job.RepairOrder.Vehicle.ColorId,
                    CreatedAt = job.RepairOrder.Vehicle.CreatedAt
                } : null,
                Customer = job.RepairOrder?.Vehicle?.User != null ? new
                {
                    CustomerId = job.RepairOrder.Vehicle.User.Id,
                    FullName = $"{job.RepairOrder.Vehicle.User.FirstName} {job.RepairOrder.Vehicle.User.LastName}".Trim(),
                    Email = job.RepairOrder.Vehicle.User.Email,
                    PhoneNumber = job.RepairOrder.Vehicle.User.PhoneNumber,
                    IsActive = job.RepairOrder.Vehicle.User.IsActive,
                    CreatedAt = job.RepairOrder.Vehicle.User.CreatedAt
                } : null,
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
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    ActualTime = r.ActualTime,
                    EstimatedTime = r.EstimatedTime,
                    Notes = r.Notes
                }).ToList()
            }).AsQueryable();

            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết một công việc cụ thể
        /// </summary>
        [HttpGet("my-jobs/{jobId}")]
        [Authorize]
        [EnableQuery]
        public async Task<IActionResult> GetJobById(Guid jobId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { Message = "Bạn cần đăng nhập." });
            }

            var isTechnician = await _userManager.IsInRoleAsync(user, "Technician");
            if (!isTechnician)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { Message = "Bạn không có quyền truy cập." });
            }

            var jobs = await _technicianService.GetJobsByTechnicianAsync(user.Id);
            var job = jobs?.FirstOrDefault(j => j.JobId == jobId);

            if (job == null)
            {
                return NotFound(new { Message = "Không tìm thấy công việc hoặc bạn không có quyền truy cập." });
            }

            var result = new
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
                Vehicle = job.RepairOrder?.Vehicle != null ? new
                {
                    job.RepairOrder.Vehicle.VehicleId,
                    LicensePlate = job.RepairOrder.Vehicle.LicensePlate,
                    VIN = job.RepairOrder.Vehicle.VIN,
                    BrandId = job.RepairOrder.Vehicle.BrandId,
                    ModelId = job.RepairOrder.Vehicle.ModelId,
                    ColorId = job.RepairOrder.Vehicle.ColorId,
                    CreatedAt = job.RepairOrder.Vehicle.CreatedAt
                } : null,
                Customer = job.RepairOrder?.Vehicle?.User != null ? new
                {
                    CustomerId = job.RepairOrder.Vehicle.User.Id,
                    FullName = $"{job.RepairOrder.Vehicle.User.FirstName} {job.RepairOrder.Vehicle.User.LastName}".Trim(),
                    Email = job.RepairOrder.Vehicle.User.Email,
                    PhoneNumber = job.RepairOrder.Vehicle.User.PhoneNumber,
                    IsActive = job.RepairOrder.Vehicle.User.IsActive,
                    CreatedAt = job.RepairOrder.Vehicle.User.CreatedAt
                } : null,
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
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    ActualTime = r.ActualTime,
                    EstimatedTime = r.EstimatedTime,
                    Notes = r.Notes
                }).ToList()
            };

            return Ok(result);
        }
    }
}