using Dtos.Job;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Services;
using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Repositories;
using System.Security.Claims;
using Dtos.Quotations;
using Dtos.Emergency;
using Services.EmergencyRequestService;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TechnicianController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly IInspectionService _inspectionService;
        private readonly IUserRepository _userRepository;
        private readonly ITechnicianService _technicianService;
        private readonly IEmergencyRequestService _emergencyService;

        public TechnicianController(IJobService jobService, IInspectionService inspectionService, IUserRepository userRepository, ITechnicianService technicianService, IEmergencyRequestService emergencyService)
        {
            _jobService = jobService;
            _inspectionService = inspectionService;
            _userRepository = userRepository;
            _technicianService = technicianService;
            _emergencyService = emergencyService;
        }

        // GET: api/Technician/schedule
        [HttpGet("schedule")]
        [EnableQuery]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<IActionResult> GetTechnicianSchedule([FromQuery] TechnicianScheduleFilterDto filter)
        {
            try
            {
                var scheduleDtos = await _technicianService.GetAllTechnicianSchedulesAsync(filter);
                return Ok(scheduleDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving technician schedules", error = ex.Message });
            }
        }

        // GET: api/Technician/{technicianId}/schedule
        [HttpGet("{technicianId}/schedule")]
        [EnableQuery]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<IActionResult> GetScheduleForTechnician(Guid technicianId, [FromQuery] TechnicianScheduleFilterDto filter)
        {
            try
            {
                var scheduleDtos = await _technicianService.GetTechnicianScheduleAsync(technicianId, filter);
                return Ok(scheduleDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving technician schedule", error = ex.Message });
            }
        }

        // GET: api/Technician/workload
        [HttpGet("workload")]
        [EnableQuery]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<IActionResult> GetTechnicianWorkload([FromQuery] Guid? technicianId = null)
        {
            try
            {
                if (technicianId.HasValue)
                {
                    var workloadDto = await _technicianService.GetTechnicianWorkloadAsync(technicianId.Value);
                    if (workloadDto == null)
                    {
                        return NotFound(new { message = "Technician not found" });
                    }
                    return Ok(workloadDto);
                }
                else
                {
                    var workloadDtos = await _technicianService.GetAllTechnicianWorkloadsAsync();
                    return Ok(workloadDtos);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving technician workload", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
        
        // GET: api/Technician/workload-test (for debugging)
        [HttpGet("workload-test")]
        [AllowAnonymous]
        public IActionResult TestWorkloadEndpoint()
        {
            return Ok(new { 
                message = "Workload endpoint is accessible",
                timestamp = DateTime.UtcNow,
                endpoint = "/api/Technician/workload"
            });
        }

        // POST: api/Technician/assign/jobs
        [HttpPost("assign/jobs")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<ActionResult> AssignJobsToTechnician([FromBody] AssignTechnicianDto assignDto)
        {
            // Get the current user (manager) ID
            var managerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(managerId))
            {
                return BadRequest("Manager ID not found in token");
            }

            if (assignDto.JobIds == null || !assignDto.JobIds.Any())
            {
                return BadRequest("At least one job ID must be provided");
            }

            var result = await _jobService.AssignJobsToTechnicianAsync(assignDto.JobIds, assignDto.TechnicianId, managerId);
            if (!result)
            {
                return BadRequest("Failed to assign jobs to technician");
            }

            return NoContent();
        }

        // POST: api/Technician/assign/inspections
        [HttpPost("assign/inspections")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<ActionResult> AssignInspectionsToTechnician([FromBody] AssignTechnicianDto assignDto)
        {
            // For inspections, we'll assign each one individually
            foreach (var inspectionId in assignDto.JobIds) // Using JobIds property but for inspections
            {
                var result = await _inspectionService.AssignInspectionToTechnicianAsync(inspectionId, assignDto.TechnicianId);
                if (!result)
                {
                    return BadRequest($"Failed to assign inspection {inspectionId} to technician");
                }
            }

            return NoContent();
        }

        // GET: api/Technician/by-branch/{branchId}
        [HttpGet("by-branch/{branchId}")]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<IActionResult> GetTechniciansByBranchId(Guid branchId)
        {
            try
            {
                var technicians = await _jobService.GetTechniciansByBranchIdAsync(branchId);
                return Ok(technicians.Select(t => new
                {
                    t.TechnicianId,
                    t.UserId,
                    UserFullName = t.User != null ? $"{t.User.FirstName} {t.User.LastName}" : "Unknown",
                    t.Quality,
                    t.Speed,
                    t.Efficiency,
                    t.Score
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving technicians", error = ex.Message });
            }
        }

        #region Helper Methods

        private IEnumerable<Job> FilterJobs(IEnumerable<Job> jobs, TechnicianScheduleFilterDto filter)
        {
            var filteredJobs = jobs.AsQueryable();
            
            // Filter by technician ID if provided
            if (filter.TechnicianId.HasValue && filter.TechnicianId.Value != Guid.Empty)
            {
                filteredJobs = filteredJobs.Where(j => j.JobTechnicians != null && 
                    j.JobTechnicians.Any(jt => jt.TechnicianId == filter.TechnicianId.Value));
            }
            
            // Filter by status if provided
            if (filter.Status.HasValue)
            {
                filteredJobs = filteredJobs.Where(j => j.Status == filter.Status.Value);
            }
            
            // Filter by date range if provided
            if (filter.FromDate.HasValue)
            {
                filteredJobs = filteredJobs.Where(j => j.CreatedAt >= filter.FromDate.Value);
            }
            
            if (filter.ToDate.HasValue)
            {
                filteredJobs = filteredJobs.Where(j => j.CreatedAt <= filter.ToDate.Value);
            }
            
            // Filter by priority level if provided
            if (filter.PriorityLevel.HasValue)
            {
                // Since we removed the Level property, we'll filter by a default value
                filteredJobs = filteredJobs.Where(j => 1 == filter.PriorityLevel.Value);
            }
            
            // Filter by overdue only if requested
            if (filter.IsOverdueOnly.HasValue && filter.IsOverdueOnly.Value)
            {
                filteredJobs = filteredJobs.Where(j => j.Deadline.HasValue && j.Deadline.Value < DateTime.UtcNow && j.Status != BusinessObject.Enums.JobStatus.Completed);
            }
            
            return filteredJobs;
        }

        #endregion
    
        [HttpPost("location/update")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> UpdateLocation([FromBody] TechnicianLocationDto dto)
        {
            try
            {
                var techUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(techUserId)) return Unauthorized();
                dto.RecomputeRoute = true;
                var ok = await _emergencyService.UpdateTechnicianLocationAsync(techUserId, dto);
                return Ok(new { Success = ok });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}