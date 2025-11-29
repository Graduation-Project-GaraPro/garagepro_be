using Dtos.Job;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Services;
using BusinessObject;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;

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
            
            var jobDtos = new List<JobDto>();
            foreach (var job in jobs)
            {
                // Get job parts
                var jobParts = await _jobService.GetJobPartsAsync(job.JobId);
                var jobPartDtos = jobParts.Select(jobPart => new JobPartDto
                {
                    JobPartId = jobPart.JobPartId,
                    JobId = jobPart.JobId,
                    PartId = jobPart.PartId,
                    Quantity = jobPart.Quantity,
                    UnitPrice = jobPart.UnitPrice,
                    CreatedAt = jobPart.CreatedAt,
                    UpdatedAt = jobPart.UpdatedAt,
                    PartName = jobPart.Part?.Name ?? ""
                });

                var jobDto = new JobDto
                {
                    JobId = job.JobId,
                    ServiceId = job.ServiceId,
                    RepairOrderId = job.RepairOrderId,
                    JobName = job.JobName,
                    Status = job.Status,
                    Deadline = job.Deadline,
                    Note = job.Note,
                    CreatedAt = job.CreatedAt,
                    UpdatedAt = job.UpdatedAt,
                    AssignedByManagerId = job.AssignedByManagerId,
                    AssignedAt = job.AssignedAt,
                    Parts = jobPartDtos.ToList()
                };
                
                jobDtos.Add(jobDto);
            }

            return Ok(jobDtos);
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

            // Get job parts
            var jobParts = await _jobService.GetJobPartsAsync(id);
            var jobPartDtos = jobParts.Select(jobPart => new JobPartDto
            {
                JobPartId = jobPart.JobPartId,
                JobId = jobPart.JobId,
                PartId = jobPart.PartId,
                Quantity = jobPart.Quantity,
                UnitPrice = jobPart.UnitPrice,
                CreatedAt = jobPart.CreatedAt,
                UpdatedAt = jobPart.UpdatedAt,
                PartName = jobPart.Part?.Name ?? ""
            });

            var jobDto = new JobDto
            {
                JobId = job.JobId,
                ServiceId = job.ServiceId,
                RepairOrderId = job.RepairOrderId,
                JobName = job.JobName,
                Status = job.Status,
                Deadline = job.Deadline,
                Note = job.Note,
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt,
                AssignedByManagerId = job.AssignedByManagerId,
                AssignedAt = job.AssignedAt,
                Parts = jobPartDtos.ToList()
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
            
            var jobDtos = new List<JobDto>();
            foreach (var job in jobs)
            {
                // Get job parts
                var jobParts = await _jobService.GetJobPartsAsync(job.JobId);
                var jobPartDtos = jobParts.Select(jobPart => new JobPartDto
                {
                    JobPartId = jobPart.JobPartId,
                    JobId = jobPart.JobId,
                    PartId = jobPart.PartId,
                    Quantity = jobPart.Quantity,
                    UnitPrice = jobPart.UnitPrice,
                    CreatedAt = jobPart.CreatedAt,
                    UpdatedAt = jobPart.UpdatedAt,
                    PartName = jobPart.Part?.Name ?? ""
                });

                var jobDto = new JobDto
                {
                    JobId = job.JobId,
                    ServiceId = job.ServiceId,
                    RepairOrderId = job.RepairOrderId,
                    JobName = job.JobName,
                    Status = job.Status,
                    Deadline = job.Deadline,
                    Note = job.Note,
                    CreatedAt = job.CreatedAt,
                    UpdatedAt = job.UpdatedAt,
                    AssignedByManagerId = job.AssignedByManagerId,
                    AssignedAt = job.AssignedAt,
                    Parts = jobPartDtos.ToList()
                };
                
                jobDtos.Add(jobDto);
            }

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

            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            // Only update the allowed fields: Status, Note, and Deadline
            if (updateJobDto.Status.HasValue && updateJobDto.Status != job.Status)
            {
                // Validate status transition - only allow Pending <-> New
                var isValidTransition = (job.Status == JobStatus.Pending && updateJobDto.Status == JobStatus.New) ||
                                       (job.Status == JobStatus.New && updateJobDto.Status == JobStatus.Pending) ||
                                       (job.Status == updateJobDto.Status); // Allow same status
                
                if (!isValidTransition)
                {
                    return BadRequest("Only transitions between Pending and New statuses are allowed.");
                }
                
                job.Status = updateJobDto.Status.Value;
            }

            // Update note if provided
            if (updateJobDto.Note != null)
            {
                job.Note = updateJobDto.Note;
            }

            // Update deadline if provided
            if (updateJobDto.Deadline.HasValue)
            {
                job.Deadline = updateJobDto.Deadline.Value;
            }
            // If status is being set to New and no deadline is provided, calculate it
            else if (updateJobDto.Status == JobStatus.New && job.Deadline == null)
            {
                // Get the service to calculate deadline from EstimatedDuration
                var service = await _jobService.GetServiceByIdAsync(job.ServiceId);
                if (service != null && service.EstimatedDuration > 0)
                {
                    job.Deadline = DateTime.UtcNow.AddHours((double)service.EstimatedDuration);
                }
            }

            job.UpdatedAt = DateTime.UtcNow;

            try
            {
                var updatedJob = await _jobService.UpdateJobAsync(job);
                return Ok(updatedJob);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the job: {ex.Message}");
            }
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

            try
            {
                var result = await _jobService.AssignJobsToTechnicianAsync(new List<Guid> { id }, technicianId, managerId);
                if (!result)
                {
                    return NotFound("Job not found or could not be assigned");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while assigning the job: {ex.Message}");
            }
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

            try
            {
                var result = await _jobService.AssignJobsToTechnicianAsync(assignDto.JobIds, assignDto.TechnicianId, managerId);
                if (!result)
                {
                    return BadRequest("Failed to assign jobs to technician");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while assigning jobs: {ex.Message}");
            }
        }

        //// PUT: api/Job/{id}/reassign/{technicianId}
        //[HttpPut("{id}/reassign/{technicianId}")]
        //[Authorize(Policy = "BOOKING_MANAGE")]
        //public async Task<ActionResult> ReassignJobToTechnician(Guid id, Guid technicianId)
        //{
        //    // Get the current user (manager) ID
        //    var managerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(managerId))
        //    {
        //        return BadRequest("Manager ID not found in token");
        //    }

        //    try
        //    {
        //        var result = await _jobService.ReassignJobToTechnicianAsync(id, technicianId, managerId);
        //        if (!result)
        //        {
        //            return NotFound("Job not found or could not be reassigned");
        //        }

        //        return NoContent();
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred while reassigning the job: {ex.Message}");
        //    }
        //}
       

        // GET: api/Job/{id}/parts
        [HttpGet("{id}/parts")]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<ActionResult> GetJobParts(Guid id)
        {
            try
            {
                var jobParts = await _jobService.GetJobPartsAsync(id);
                var jobPartDtos = jobParts.Select(jobPart => new JobPartDto
                {
                    JobPartId = jobPart.JobPartId,
                    JobId = jobPart.JobId,
                    PartId = jobPart.PartId,
                    Quantity = jobPart.Quantity,
                    UnitPrice = jobPart.UnitPrice,
                    CreatedAt = jobPart.CreatedAt,
                    UpdatedAt = jobPart.UpdatedAt,
                    PartName = jobPart.Part?.Name ?? ""
                });
                return Ok(jobPartDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching job parts: {ex.Message}");
            }
        }
    }
}