using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;
using Repositories;

namespace Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;

        public JobService(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        #region Basic CRUD Operations

        public async Task<Job?> GetJobByIdAsync(Guid jobId)
        {
            return await _jobRepository.GetByIdAsync(jobId);
        }

        public async Task<Job> CreateJobAsync(Job job)
        {
            // Business validation before creation
            if (string.IsNullOrWhiteSpace(job.JobName))
                throw new ArgumentException("Job name is required", nameof(job.JobName));

            if (job.ServiceId == Guid.Empty)
                throw new ArgumentException("Service ID is required", nameof(job.ServiceId));

            if (job.RepairOrderId == Guid.Empty)
                throw new ArgumentException("Repair Order ID is required", nameof(job.RepairOrderId));

            // Set default values
            job.Status = JobStatus.Pending;
            job.CreatedAt = DateTime.UtcNow;
            job.UpdatedAt = null;

            return await _jobRepository.CreateAsync(job);
        }

        public async Task<Job> UpdateJobAsync(Job job)
        {
            var existingJob = await _jobRepository.GetByIdAsync(job.JobId);
            if (existingJob == null)
                throw new ArgumentException("Job not found", nameof(job.JobId));

            // Preserve audit fields
            job.CreatedAt = existingJob.CreatedAt;
            job.UpdatedAt = DateTime.UtcNow;

            return await _jobRepository.UpdateAsync(job);
        }

        public async Task<bool> DeleteJobAsync(Guid jobId)
        {
            // Validate if job can be deleted
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return false;

            // Business rule: Can only delete jobs that are not in progress or completed
            if (job.Status == JobStatus.InProgress || job.Status == JobStatus.Completed)
                throw new InvalidOperationException("Cannot delete jobs that are in progress or completed");

            return await _jobRepository.DeleteAsync(jobId);
        }

        public async Task<bool> JobExistsAsync(Guid jobId)
        {
            return await _jobRepository.ExistsAsync(jobId);
        }

        // NEW: Get all jobs with OData support
        public async Task<IEnumerable<Job>> GetAllJobsAsync()
        {
            return await _jobRepository.GetJobsWithNavigationPropertiesAsync();
        }

        // NEW: Get jobs by status
        public async Task<IEnumerable<Job>> GetJobsByStatusIdAsync(JobStatus status)
        {
            return await _jobRepository.GetJobsByStatusAsync(status);
        }

        #endregion

        #region Job Queries by Context

        public async Task<IEnumerable<Job>> GetJobsByRepairOrderIdAsync(Guid repairOrderId)
        {
            return await _jobRepository.GetJobsByRepairOrderIdAsync(repairOrderId);
        }


        public async Task<IEnumerable<Job>> GetJobsByStatusAsync(JobStatus status)
        {
            return await _jobRepository.GetJobsByStatusAsync(status);
        }

        #endregion


        #region Manager Assignment Workflow

        public async Task<bool> AssignJobsToTechnicianAsync(List<Guid> jobIds, Guid technicianId, string managerId)
        {
            if (jobIds == null || !jobIds.Any())
                throw new ArgumentException("Job IDs cannot be null or empty", nameof(jobIds));

            if (technicianId == Guid.Empty)
                throw new ArgumentException("Technician ID is required", nameof(technicianId));

            if (string.IsNullOrWhiteSpace(managerId))
                throw new ArgumentException("Manager ID is required", nameof(managerId));

            // Validate all jobs can be assigned
            foreach (var jobId in jobIds)
            {
                if (!await CanAssignJobToTechnicianAsync(jobId))
                    throw new InvalidOperationException($"Job {jobId} cannot be assigned to technician");
            }

            return await _jobRepository.AssignJobsToTechnicianAsync(jobIds, technicianId, managerId);
        }

        public async Task<bool> ReassignJobToTechnicianAsync(Guid jobId, Guid newTechnicianId, string managerId)
        {
            if (jobId == Guid.Empty)
                throw new ArgumentException("Job ID is required", nameof(jobId));

            if (newTechnicianId == Guid.Empty)
                throw new ArgumentException("Technician ID is required", nameof(newTechnicianId));

            if (string.IsNullOrWhiteSpace(managerId))
                throw new ArgumentException("Manager ID is required", nameof(managerId));

            return await _jobRepository.ReassignJobToTechnicianAsync(jobId, newTechnicianId, managerId);
        }





        #endregion


        #region Job Parts Management

        public async Task<IEnumerable<JobPart>> GetJobPartsAsync(Guid jobId)
        {
            return await _jobRepository.GetJobPartsAsync(jobId);
        }

        public async Task<bool> AddJobPartAsync(JobPart jobPart)
        {
            // Validation
            if (jobPart.JobId == Guid.Empty)
                throw new ArgumentException("Job ID is required", nameof(jobPart.JobId));

            if (jobPart.PartId == Guid.Empty)
                throw new ArgumentException("Part ID is required", nameof(jobPart.PartId));

            if (jobPart.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(jobPart.Quantity));

            if (jobPart.UnitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative", nameof(jobPart.UnitPrice));

            jobPart.CreatedAt = DateTime.UtcNow;
            return await _jobRepository.AddJobPartAsync(jobPart);
        }

        public async Task<bool> UpdateJobPartAsync(JobPart jobPart)
        {
            if (jobPart.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(jobPart.Quantity));

            if (jobPart.UnitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative", nameof(jobPart.UnitPrice));

            return await _jobRepository.UpdateJobPartAsync(jobPart);
        }

        public async Task<bool> RemoveJobPartAsync(Guid jobPartId)
        {
            return await _jobRepository.RemoveJobPartAsync(jobPartId);
        }

        public async Task<decimal> CalculateJobTotalAmountAsync(Guid jobId)
        {
            return await _jobRepository.CalculateJobTotalAmountAsync(jobId);
        }

        #endregion


        #region Status Management

        public async Task<bool> UpdateJobStatusAsync(Guid jobId, JobStatus newStatus, string? changeNote = null)
        {
            // Validate workflow transition
            if (!await ValidateJobWorkflowAsync(jobId, newStatus))
                throw new InvalidOperationException($"Invalid status transition to {newStatus}");

            return await _jobRepository.UpdateJobStatusAsync(jobId, newStatus, changeNote);
        }

        public async Task<bool> BatchUpdateStatusAsync(List<(Guid JobId, JobStatus NewStatus, string? ChangeNote)> updates)
        {
            if (updates == null || !updates.Any())
                throw new ArgumentException("Updates cannot be null or empty", nameof(updates));
            // Validate all transitions
            foreach (var (jobId, newStatus, _) in updates)
            {
                if (!await ValidateJobWorkflowAsync(jobId, newStatus))
                    throw new InvalidOperationException($"Invalid status transition for job {jobId} to {newStatus}");
            }

            return await _jobRepository.BatchUpdateStatusAsync(updates);
        }

        #endregion

        #region Business Logic Validation


        public async Task<bool> CanSendJobToCustomerAsync(Guid jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return false;

            // Job must be in Pending status to be sent to customer
            return job.Status == JobStatus.Pending;
        }

        public async Task<bool> CanAssignJobToTechnicianAsync(Guid jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return false;

            // Job must be in Pending status to be assigned to technician
            return job.Status == JobStatus.Pending;
        }

        #endregion

        #region Search and Filtering

        public async Task<IEnumerable<Job>> SearchJobsAsync(
            string? searchText = null,
            List<JobStatus>? statuses = null,
            List<Guid>? repairOrderIds = null,
            List<Guid>? serviceIds = null,
            List<Guid>? technicianIds = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            return await _jobRepository.SearchJobsAsync(searchText, statuses, repairOrderIds, serviceIds, technicianIds, fromDate, toDate);
        }

        #endregion

        #region Workflow Validation

        public async Task<bool> ValidateJobWorkflowAsync(Guid jobId, JobStatus targetStatus)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return false;

            return IsValidTransition(job.Status, targetStatus);
        }

        public async Task<string> GetNextAllowedStatusesAsync(Guid jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return "Job not found";

            var allowedStatuses = GetAllowedNextStatuses(job.Status);
            return string.Join(", ", allowedStatuses);
        }

        #endregion

        #region Private Helper Methods

        private static bool IsValidTransition(JobStatus currentStatus, JobStatus targetStatus)
        {
            var allowedTransitions = new Dictionary<JobStatus, List<JobStatus>>
            {
               // [JobStatus.Pending] = new List<JobStatus> { JobStatus.AssignedToTechnician },
               //[JobStatus.AssignedToTechnician] = new List<JobStatus> { JobStatus.InProgress },
               // [JobStatus.InProgress] = new List<JobStatus> { JobStatus.Completed, JobStatus.AssignedToTechnician }, // Can be reassigned
                [JobStatus.Completed] = new List<JobStatus>() // Terminal status
            };

            return allowedTransitions.ContainsKey(currentStatus) &&
                   allowedTransitions[currentStatus].Contains(targetStatus);
        }

        private static List<JobStatus> GetAllowedNextStatuses(JobStatus currentStatus)
        {
            var allowedTransitions = new Dictionary<JobStatus, List<JobStatus>>
            {
                //[JobStatus.Pending] = new List<JobStatus> { JobStatus.AssignedToTechnician },
                //[JobStatus.AssignedToTechnician] = new List<JobStatus> { JobStatus.InProgress },
                //[JobStatus.InProgress] = new List<JobStatus> { JobStatus.Completed, JobStatus.AssignedToTechnician },
                [JobStatus.Completed] = new List<JobStatus>()
            };

            return allowedTransitions.ContainsKey(currentStatus) ?
                   allowedTransitions[currentStatus] :
                   new List<JobStatus>();
        }

        #endregion
        
    }
}