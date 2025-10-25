using Dtos.Job;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Services;
using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        // GET: api/Job
        [HttpGet]
        [EnableQuery]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<IActionResult> GetJobs()
        {
            var jobs = await _jobService.GetAllJobsAsync();
            return Ok(jobs);
        }

        // GET: api/Job/5
        [HttpGet("{id}")]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<IActionResult> GetJob(Guid id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            var jobDto = new JobDto
            {
                JobId = job.JobId,
                ServiceId = job.ServiceId,
                RepairOrderId = job.RepairOrderId,
                JobName = job.JobName,
                Status = job.Status,
                Deadline = job.Deadline,
                TotalAmount = job.TotalAmount,
                Note = job.Note,
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt,
                Level = job.Level,
                SentToCustomerAt = job.SentToCustomerAt,
                CustomerResponseAt = job.CustomerResponseAt,
                CustomerApprovalNote = job.CustomerApprovalNote,
                AssignedByManagerId = job.AssignedByManagerId,
                AssignedAt = job.AssignedAt,
                EstimateExpiresAt = job.EstimateExpiresAt,
                RevisionCount = job.RevisionCount,
                OriginalJobId = job.OriginalJobId,
                RevisionReason = job.RevisionReason
            };

            return Ok(jobDto);
        }

        // GET: api/Job/repairorder/5
        [HttpGet("repairorder/{repairOrderId}")]
        [EnableQuery]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<IActionResult> GetJobsByRepairOrder(Guid repairOrderId)
        {
            var jobs = await _jobService.GetJobsByRepairOrderIdAsync(repairOrderId);
            var jobDtos = jobs.Select(job => new JobDto
            {
                JobId = job.JobId,
                ServiceId = job.ServiceId,
                RepairOrderId = job.RepairOrderId,
                JobName = job.JobName,
                Status = job.Status,
                Deadline = job.Deadline,
                TotalAmount = job.TotalAmount,
                Note = job.Note,
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt,
                Level = job.Level,
                SentToCustomerAt = job.SentToCustomerAt,
                CustomerResponseAt = job.CustomerResponseAt,
                CustomerApprovalNote = job.CustomerApprovalNote,
                AssignedByManagerId = job.AssignedByManagerId,
                AssignedAt = job.AssignedAt,
                EstimateExpiresAt = job.EstimateExpiresAt,
                RevisionCount = job.RevisionCount,
                OriginalJobId = job.OriginalJobId,
                RevisionReason = job.RevisionReason
            });

            return Ok(jobDtos);
        }

        // POST: api/Job
        [HttpPost]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<IActionResult> CreateJob(CreateJobDto createJobDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Implementation would go here
            return CreatedAtAction(nameof(GetJob), new { id = Guid.NewGuid() }, createJobDto);
        }

        // PUT: api/Job/5
        [HttpPut("{id}")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<IActionResult> UpdateJob(Guid id, UpdateJobDto updateJobDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Implementation would go here
            return NoContent();
        }

        // DELETE: api/Job/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<IActionResult> DeleteJob(Guid id)
        {
            var result = await _jobService.DeleteJobAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Job/5/status
        [HttpPut("{id}/status")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<IActionResult> UpdateJobStatus(Guid id, [FromBody] string status)
        {
            // Convert string to JobStatus enum
            if (!Enum.TryParse<BusinessObject.Enums.JobStatus>(status, out var jobStatus))
            {
                return BadRequest("Invalid job status");
            }

            var result = await _jobService.UpdateJobStatusAsync(id, jobStatus);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        
        // GET: api/Job/status/{status}
        [HttpGet("status/{status}")]
        [EnableQuery]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<IActionResult> GetJobsByStatus(string status)
        {
            // Convert string to JobStatus enum
            if (!Enum.TryParse<BusinessObject.Enums.JobStatus>(status, out var jobStatus))
            {
                return BadRequest("Invalid job status");
            }
            
            var jobs = await _jobService.GetJobsByStatusIdAsync(jobStatus);
            return Ok(jobs);
        }

        // PUT: api/Job/{id}/assign/{technicianId}
        [HttpPut("{id}/assign/{technicianId}")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<ActionResult> AssignJobToTechnician(Guid id, Guid technicianId)
        {
            // Get the current user (manager) ID
            var managerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(managerId))
            {
                return BadRequest("Manager ID not found in token");
            }

            var result = await _jobService.AssignJobsToTechnicianAsync(new List<Guid> { id }, technicianId, managerId);
            if (!result)
            {
                return NotFound("Job not found or could not be assigned");
            }

            return NoContent();
        }

        // POST: api/Job/assign
        [HttpPost("assign")]
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

        // PUT: api/Job/{id}/reassign/{technicianId}
        [HttpPut("{id}/reassign/{technicianId}")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<ActionResult> ReassignJobToTechnician(Guid id, Guid technicianId)
        {
            // Get the current user (manager) ID
            var managerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(managerId))
            {
                return BadRequest("Manager ID not found in token");
            }

            var result = await _jobService.ReassignJobToTechnicianAsync(id, technicianId, managerId);
            if (!result)
            {
                return NotFound("Job not found or could not be reassigned");
            }

            return NoContent();
        }

        // PUT: api/Job/{id}/start
        [HttpPut("{id}/start")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<ActionResult> MarkJobAsInProgress(Guid id)
        {
            // Get the current user (technician) ID - this will be used to ensure they're assigned to the job
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in token");
            }

            // For now, we'll use a placeholder technician ID
            // In a real implementation, you'd get the actual technician ID from the user
            var technicianId = Guid.NewGuid(); // This should be replaced with actual logic
            
            var result = await _jobService.MarkJobAsInProgressAsync(id, technicianId);
            if (!result)
            {
                return NotFound("Job not found or could not be started");
            }

            return NoContent();
        }
    }
}