using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.Technician;
using Repositories;

namespace Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IQuotationRepository _quotationRepository; // Add this

        public JobService(IJobRepository jobRepository, IQuotationRepository quotationRepository) // Update constructor
        {
            _jobRepository = jobRepository;
            _quotationRepository = quotationRepository;
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

        public async Task<IEnumerable<Job>> GetJobsByServiceIdAsync(Guid serviceId)
        {
            return await _jobRepository.GetJobsByServiceIdAsync(serviceId);
        }

        public async Task<IEnumerable<Job>> GetJobsByStatusAsync(JobStatus status)
        {
            return await _jobRepository.GetJobsByStatusAsync(status);
        }

        public async Task<Job?> GetJobWithFullDetailsAsync(Guid jobId)
        {
            return await _jobRepository.GetJobWithFullDetailsAsync(jobId);
        }

        public async Task<IEnumerable<Job>> GetPendingJobsByRepairOrderIdAsync(Guid repairOrderId)
        {
            if (repairOrderId == Guid.Empty)
                throw new ArgumentException("Repair Order ID is required", nameof(repairOrderId));

            return await _jobRepository.GetPendingJobsByRepairOrderIdAsync(repairOrderId);
        }

        #endregion

        #region Customer Approval Workflow

        public async Task<bool> SendJobsToCustomerForApprovalAsync(List<Guid> jobIds, string managerId)
        {
            if (jobIds == null || !jobIds.Any())
                throw new ArgumentException("Job IDs cannot be null or empty", nameof(jobIds));

            if (string.IsNullOrWhiteSpace(managerId))
                throw new ArgumentException("Manager ID is required", nameof(managerId));

            // Validate all jobs can be sent to customer
            foreach (var jobId in jobIds)
            {
                if (!await CanSendJobToCustomerAsync(jobId))
                    throw new InvalidOperationException($"Job {jobId} cannot be sent to customer");
            }

            return await _jobRepository.SendJobsToCustomerForApprovalAsync(jobIds, managerId);
        }

        

        public async Task<bool> ProcessCustomerApprovalAsync(Guid jobId, bool isApproved, string? customerNote = null)
        {
            if (jobId == Guid.Empty)
                throw new ArgumentException("Job ID is required", nameof(jobId));

            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
                throw new ArgumentException("Job not found", nameof(jobId));

            if (job.Status != JobStatus.WaitingCustomerApproval)
                throw new InvalidOperationException("Job is not waiting for customer approval");

            return await _jobRepository.ProcessCustomerApprovalAsync(jobId, isApproved, customerNote);
        }

        public async Task<IEnumerable<Job>> GetJobsWaitingCustomerApprovalAsync(Guid repairOrderId)
        {
            return await _jobRepository.GetJobsWaitingCustomerApprovalAsync(repairOrderId);
        }

        public async Task<IEnumerable<Job>> GetJobsApprovedByCustomerAsync(Guid? repairOrderId = null)
        {
            return await _jobRepository.GetJobsApprovedByCustomerAsync(repairOrderId);
        }

        public async Task<IEnumerable<Job>> GetJobsRejectedByCustomerAsync(Guid? repairOrderId = null)
        {
            return await _jobRepository.GetJobsRejectedByCustomerAsync(repairOrderId);
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

        public async Task<IEnumerable<Job>> GetJobsReadyForAssignmentAsync(Guid? repairOrderId = null)
        {
            return await _jobRepository.GetJobsReadyForAssignmentAsync(repairOrderId);
        }

        public async Task<IEnumerable<Job>> GetJobsAssignedByManagerAsync(string managerId)
        {
            if (string.IsNullOrWhiteSpace(managerId))
                throw new ArgumentException("Manager ID is required", nameof(managerId));

            return await _jobRepository.GetJobsAssignedByManagerAsync(managerId);
        }

        public async Task<IEnumerable<Job>> GetJobsByTechnicianIdAsync(Guid technicianId)
        {
            return await _jobRepository.GetJobsByTechnicianIdAsync(technicianId);
        }

        public async Task<IEnumerable<Job>> GetUnassignedJobsAsync()
        {
            return await _jobRepository.GetUnassignedJobsAsync();
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

        #region Repair Activities Management

        public async Task<IEnumerable<Repair>> GetJobRepairsAsync(Guid jobId)
        {
            return await _jobRepository.GetJobRepairsAsync(jobId);
        }

        public async Task<Repair?> GetActiveRepairForJobAsync(Guid jobId)
        {
            return await _jobRepository.GetActiveRepairForJobAsync(jobId);
        }

        public async Task<bool> StartRepairForJobAsync(Guid jobId, Repair repair)
        {
            // Validate job can start repair
            if (!await CanStartJobAsync(jobId))
                throw new InvalidOperationException("Job cannot start repair at this time");

            // Validate repair details
            if (repair == null)
                throw new ArgumentNullException(nameof(repair));

            return await _jobRepository.StartRepairForJobAsync(jobId, repair);
        }

        public async Task<bool> CompleteRepairForJobAsync(Guid repairId, string? notes = null)
        {
            return await _jobRepository.CompleteRepairForJobAsync(repairId, notes);
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

        public async Task<IEnumerable<Job>> BatchUpdateStatusAsync(List<(Guid JobId, JobStatus NewStatus, string? ChangeNote)> updates)
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

        public async Task<bool> CanCompleteJobAsync(Guid jobId)
        {
            return await _jobRepository.CanCompleteJobAsync(jobId);
        }

        public async Task<bool> CanStartJobAsync(Guid jobId)
        {
            return await _jobRepository.CanStartJobAsync(jobId);
        }

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

            // Job must be approved by customer to be assigned to technician
            return job.Status == JobStatus.CustomerApproved;
        }

        public async Task<bool> HasActiveTechnicianAsync(Guid jobId)
        {
            return await _jobRepository.HasActiveTechnicianAsync(jobId);
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

        #region Statistics and Reporting

        public async Task<Dictionary<JobStatus, int>> GetJobCountsByStatusAsync(List<Guid>? repairOrderIds = null)
        {
            return await _jobRepository.GetJobCountsByStatusAsync(repairOrderIds);
        }

        public async Task<Dictionary<JobStatus, int>> GetJobStatusCountsByRepairOrderAsync(Guid repairOrderId)
        {
            return await _jobRepository.GetJobStatusCountsByRepairOrderAsync(repairOrderId);
        }

        public async Task<Dictionary<string, object>> GetJobStatisticsAsync(Guid? repairOrderId = null)
        {
            return await _jobRepository.GetJobStatisticsAsync(repairOrderId);
        }

        public async Task<IEnumerable<Job>> GetOverdueJobsAsync()
        {
            return await _jobRepository.GetOverdueJobsAsync();
        }

        public async Task<IEnumerable<Job>> GetJobsDueWithinDaysAsync(int days)
        {
            if (days < 0)
                throw new ArgumentException("Days must be non-negative", nameof(days));

            return await _jobRepository.GetJobsDueWithinDaysAsync(days);
        }

        public async Task<IEnumerable<Job>> GetHighPriorityJobsAsync(int minLevel = 5)
        {
            return await _jobRepository.GetHighPriorityJobsAsync(minLevel);
        }

        #endregion

        #region Level and Priority Management

        public async Task<IEnumerable<Job>> GetJobsByLevelAsync(int level)
        {
            return await _jobRepository.GetJobsByLevelAsync(level);
        }

        public async Task<bool> UpdateJobLevelAsync(Guid jobId, int newLevel)
        {
            if (newLevel < 0)
                throw new ArgumentException("Level cannot be negative", nameof(newLevel));

            return await _jobRepository.UpdateJobLevelAsync(jobId, newLevel);
        }

        #endregion

        #region Completion Tracking

        public async Task<bool> MarkJobAsCompletedAsync(Guid jobId, string? completionNotes = null)
        {
            return await _jobRepository.MarkJobAsCompletedAsync(jobId, completionNotes);
        }

        public async Task<bool> MarkJobAsInProgressAsync(Guid jobId, Guid technicianId)
        {
            return await _jobRepository.MarkJobAsInProgressAsync(jobId, technicianId);
        }

        public async Task<TimeSpan?> GetJobDurationAsync(Guid jobId)
        {
            return await _jobRepository.GetJobDurationAsync(jobId);
        }

        public async Task<decimal> GetJobProgressPercentageAsync(Guid jobId)
        {
            return await _jobRepository.GetJobProgressPercentageAsync(jobId);
        }

        #endregion

        #region Audit and History

        public async Task<IEnumerable<Job>> GetRecentlyUpdatedJobsAsync(int hours = 24)
        {
            if (hours < 0)
                throw new ArgumentException("Hours must be non-negative", nameof(hours));

            return await _jobRepository.GetRecentlyUpdatedJobsAsync(hours);
        }

        public async Task<DateTime?> GetLastStatusChangeAsync(Guid jobId)
        {
            return await _jobRepository.GetLastStatusChangeAsync(jobId);
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
                [JobStatus.Pending] = new List<JobStatus> { JobStatus.WaitingCustomerApproval },
                [JobStatus.WaitingCustomerApproval] = new List<JobStatus> { JobStatus.CustomerApproved, JobStatus.CustomerRejected },
                [JobStatus.CustomerApproved] = new List<JobStatus> { JobStatus.AssignedToTechnician },
                [JobStatus.CustomerRejected] = new List<JobStatus> { JobStatus.Pending }, // Can be revised and resent
                [JobStatus.AssignedToTechnician] = new List<JobStatus> { JobStatus.InProgress },
                [JobStatus.InProgress] = new List<JobStatus> { JobStatus.Completed, JobStatus.AssignedToTechnician }, // Can be reassigned
                [JobStatus.Completed] = new List<JobStatus>() // Terminal status
            };

            return allowedTransitions.ContainsKey(currentStatus) && 
                   allowedTransitions[currentStatus].Contains(targetStatus);
        }

        private static List<JobStatus> GetAllowedNextStatuses(JobStatus currentStatus)
        {
            var allowedTransitions = new Dictionary<JobStatus, List<JobStatus>>
            {
                [JobStatus.Pending] = new List<JobStatus> { JobStatus.WaitingCustomerApproval },
                [JobStatus.WaitingCustomerApproval] = new List<JobStatus> { JobStatus.CustomerApproved, JobStatus.CustomerRejected },
                [JobStatus.CustomerApproved] = new List<JobStatus> { JobStatus.AssignedToTechnician },
                [JobStatus.CustomerRejected] = new List<JobStatus> { JobStatus.Pending },
                [JobStatus.AssignedToTechnician] = new List<JobStatus> { JobStatus.InProgress },
                [JobStatus.InProgress] = new List<JobStatus> { JobStatus.Completed, JobStatus.AssignedToTechnician },
                [JobStatus.Completed] = new List<JobStatus>()
            };

            return allowedTransitions.ContainsKey(currentStatus) ? 
                   allowedTransitions[currentStatus] : 
                   new List<JobStatus>();
        }

        #endregion
        
        #region Estimate Expiration and Revision Management

        public async Task<bool> SetJobEstimateExpirationAsync(Guid jobId, int expirationDays)
        {
            if (expirationDays <= 0)
                throw new ArgumentException("Expiration days must be greater than zero", nameof(expirationDays));

            return await _jobRepository.SetJobEstimateExpirationAsync(jobId, expirationDays);
        }

        public async Task<IEnumerable<Job>> GetExpiredEstimatesAsync()
        {
            return await _jobRepository.GetExpiredEstimatesAsync();
        }

        public async Task<bool> IsEstimateExpiredAsync(Guid jobId)
        {
            return await _jobRepository.IsEstimateExpiredAsync(jobId);
        }

        public async Task<Job> ReviseJobEstimateAsync(Guid originalJobId, string managerId, string revisionReason)
        {
            if (originalJobId == Guid.Empty)
                throw new ArgumentException("Original job ID is required", nameof(originalJobId));

            if (string.IsNullOrWhiteSpace(managerId))
                throw new ArgumentException("Manager ID is required", nameof(managerId));

            if (string.IsNullOrWhiteSpace(revisionReason))
                throw new ArgumentException("Revision reason is required", nameof(revisionReason));

            return await _jobRepository.ReviseJobEstimateAsync(originalJobId, managerId, revisionReason);
        }

        public async Task<IEnumerable<Job>> GetJobRevisionsAsync(Guid originalJobId)
        {
            return await _jobRepository.GetJobRevisionsAsync(originalJobId);
        }

        public async Task<Job?> GetLatestJobRevisionAsync(Guid originalJobId)
        {
            return await _jobRepository.GetLatestJobRevisionAsync(originalJobId);
        }

        public async Task<bool> ExpireOldEstimatesAsync()
        {
            return await _jobRepository.ExpireOldEstimatesAsync();
        }

        public async Task<Job> CreateJobFromServiceAsync(Guid serviceId, Guid repairOrderId, string managerId)
        {
            if (serviceId == Guid.Empty)
                throw new ArgumentException("Service ID is required", nameof(serviceId));

            if (repairOrderId == Guid.Empty)
                throw new ArgumentException("Repair Order ID is required", nameof(repairOrderId));

            if (string.IsNullOrWhiteSpace(managerId))
                throw new ArgumentException("Manager ID is required", nameof(managerId));

            // Get service details to create job
            // Note: We would need IServiceRepository injected to get service details
            // For now, creating basic job structure
            
            var job = new Job
            {
                ServiceId = serviceId,
                RepairOrderId = repairOrderId,
                JobName = $"Service Job - {DateTime.UtcNow:yyyy-MM-dd}", // Default name, can be updated
                Status = JobStatus.Pending,
                Level = 1, // Default priority
                CreatedAt = DateTime.UtcNow,
                Note = $"Job created by manager {managerId}"
                // TotalAmount will be calculated when parts are added or from Service.Price
            };

            return await _jobRepository.CreateAsync(job);
        }
        
        public async Task<Job> CreateJobFromQuotationAsync(Guid quotationId, Guid serviceId, string managerId)
        {
            if (quotationId == Guid.Empty)
                throw new ArgumentException("Quotation ID is required", nameof(quotationId));

            if (serviceId == Guid.Empty)
                throw new ArgumentException("Service ID is required", nameof(serviceId));

            if (string.IsNullOrWhiteSpace(managerId))
                throw new ArgumentException("Manager ID is required", nameof(managerId));

            // Get quotation details
            var quotation = await _quotationRepository.GetByIdAsync(quotationId);
            if (quotation == null)
                throw new ArgumentException("Quotation not found", nameof(quotationId));

            // Verify the quotation is approved
            if (quotation.Status != QuotationStatus.Approved)
                throw new InvalidOperationException("Quotation must be approved before creating a job");

            // Verify the service exists in the quotation
            var quotationService = quotation.QuotationServices.FirstOrDefault(qs => qs.ServiceId == serviceId);
            if (quotationService == null)
                throw new ArgumentException("Service not found in quotation", nameof(serviceId));

            // Create job from quotation
            var job = new Job
            {
                ServiceId = serviceId,
                RepairOrderId = quotation.Inspection.RepairOrderId, // Get RepairOrderId from inspection
                JobName = $"Job from Quotation - {quotationService.Service.ServiceName}", // Use service name
                Status = JobStatus.Pending,
                Level = 1, // Default priority
                CreatedAt = DateTime.UtcNow,
                Note = $"Job created from quotation #{quotation.QuotationId} by manager {managerId}",
                QuotationId = quotationId, // Link to the quotation
                TotalAmount = quotationService.ServicePrice // Set initial amount from quotation
                // Additional job parts will be added separately based on QuotationServicePart
            };

            return await _jobRepository.CreateAsync(job);
        }

        #endregion
    }
}