using Dtos.Job;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Services;
using BusinessObject;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<IActionResult> GetJobs()
        {
            var jobs = await _jobService.GetAllJobsAsync();
            return Ok(jobs);
        }

        // GET: api/Job/5
        [HttpGet("{id}")]
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
    }
}