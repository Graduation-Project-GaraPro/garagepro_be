using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.Technician;

namespace Services
{
    public interface IJobService
    {
        // Basic CRUD Operations
        Task<Job?> GetJobByIdAsync(Guid jobId);
        Task<Job> CreateJobAsync(Job job);
        Task<Job> UpdateJobAsync(Job job);
        Task<bool> DeleteJobAsync(Guid jobId);
        Task<bool> JobExistsAsync(Guid jobId);

        // Job Queries by Context
        Task<IEnumerable<Job>> GetJobsByRepairOrderIdAsync(Guid repairOrderId);
        Task<IEnumerable<Job>> GetJobsByServiceIdAsync(Guid serviceId);
        Task<IEnumerable<Job>> GetJobsByStatusAsync(JobStatus status);
        Task<Job?> GetJobWithFullDetailsAsync(Guid jobId);
        Task<IEnumerable<Job>> GetPendingJobsByRepairOrderIdAsync(Guid repairOrderId);

        // Customer Approval Workflow
        Task<bool> SendJobsToCustomerForApprovalAsync(List<Guid> jobIds, string managerId);
        Task<bool> ProcessCustomerApprovalAsync(Guid jobId, bool isApproved, string? customerNote = null);
        Task<IEnumerable<Job>> GetJobsWaitingCustomerApprovalAsync(Guid repairOrderId);
        Task<IEnumerable<Job>> GetJobsApprovedByCustomerAsync(Guid? repairOrderId = null);
        Task<IEnumerable<Job>> GetJobsRejectedByCustomerAsync(Guid? repairOrderId = null);

        // Manager Assignment Workflow
        Task<bool> AssignJobsToTechnicianAsync(List<Guid> jobIds, Guid technicianId, string managerId);
        Task<bool> ReassignJobToTechnicianAsync(Guid jobId, Guid newTechnicianId, string managerId);
        Task<IEnumerable<Job>> GetJobsReadyForAssignmentAsync(Guid? repairOrderId = null);
        Task<IEnumerable<Job>> GetJobsAssignedByManagerAsync(string managerId);
        Task<IEnumerable<Job>> GetJobsByTechnicianIdAsync(Guid technicianId);
        Task<IEnumerable<Job>> GetUnassignedJobsAsync();

        // Job Parts Management
        Task<IEnumerable<JobPart>> GetJobPartsAsync(Guid jobId);
        Task<bool> AddJobPartAsync(JobPart jobPart);
        Task<bool> UpdateJobPartAsync(JobPart jobPart);
        Task<bool> RemoveJobPartAsync(Guid jobPartId);
        Task<decimal> CalculateJobTotalAmountAsync(Guid jobId);

        // Repair Activities Management
        Task<IEnumerable<Repair>> GetJobRepairsAsync(Guid jobId);
        Task<Repair?> GetActiveRepairForJobAsync(Guid jobId);
        Task<bool> StartRepairForJobAsync(Guid jobId, Repair repair);
        Task<bool> CompleteRepairForJobAsync(Guid repairId, string? notes = null);

        // Status Management
        Task<bool> UpdateJobStatusAsync(Guid jobId, JobStatus newStatus, string? changeNote = null);
        Task<IEnumerable<Job>> BatchUpdateStatusAsync(List<(Guid JobId, JobStatus NewStatus, string? ChangeNote)> updates);

        // Business Logic Validation
        Task<bool> CanCompleteJobAsync(Guid jobId);
        Task<bool> CanStartJobAsync(Guid jobId);
        Task<bool> CanSendJobToCustomerAsync(Guid jobId);
        Task<bool> CanAssignJobToTechnicianAsync(Guid jobId);
        Task<bool> HasActiveTechnicianAsync(Guid jobId);

        // Search and Filtering
        Task<IEnumerable<Job>> SearchJobsAsync(
            string? searchText = null,
            List<JobStatus>? statuses = null,
            List<Guid>? repairOrderIds = null,
            List<Guid>? serviceIds = null,
            List<Guid>? technicianIds = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        // Statistics and Reporting
        Task<Dictionary<JobStatus, int>> GetJobCountsByStatusAsync(List<Guid>? repairOrderIds = null);
        Task<Dictionary<JobStatus, int>> GetJobStatusCountsByRepairOrderAsync(Guid repairOrderId);
        Task<Dictionary<string, object>> GetJobStatisticsAsync(Guid? repairOrderId = null);
        Task<IEnumerable<Job>> GetOverdueJobsAsync();
        Task<IEnumerable<Job>> GetJobsDueWithinDaysAsync(int days);
        Task<IEnumerable<Job>> GetHighPriorityJobsAsync(int minLevel = 5);

        // Level and Priority Management
        Task<IEnumerable<Job>> GetJobsByLevelAsync(int level);
        Task<bool> UpdateJobLevelAsync(Guid jobId, int newLevel);

        // Completion Tracking
        Task<bool> MarkJobAsCompletedAsync(Guid jobId, string? completionNotes = null);
        Task<bool> MarkJobAsInProgressAsync(Guid jobId, Guid technicianId);
        Task<TimeSpan?> GetJobDurationAsync(Guid jobId);
        Task<decimal> GetJobProgressPercentageAsync(Guid jobId);

        // Audit and History
        Task<IEnumerable<Job>> GetRecentlyUpdatedJobsAsync(int hours = 24);
        Task<DateTime?> GetLastStatusChangeAsync(Guid jobId);

        // Workflow Validation
        Task<bool> ValidateJobWorkflowAsync(Guid jobId, JobStatus targetStatus);
        Task<string> GetNextAllowedStatusesAsync(Guid jobId);
        
        // Estimate Expiration and Revision Management
        Task<bool> SetJobEstimateExpirationAsync(Guid jobId, int expirationDays);
        Task<IEnumerable<Job>> GetExpiredEstimatesAsync();
        Task<bool> IsEstimateExpiredAsync(Guid jobId);
        Task<Job> ReviseJobEstimateAsync(Guid originalJobId, string managerId, string revisionReason);
        Task<IEnumerable<Job>> GetJobRevisionsAsync(Guid originalJobId);
        Task<Job?> GetLatestJobRevisionAsync(Guid originalJobId);
        Task<bool> ExpireOldEstimatesAsync();
        Task<Job> CreateJobFromServiceAsync(Guid serviceId, Guid repairOrderId, string managerId);
    }
}