using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
//using BusinessObject.InspectionAndRepair;
using BusinessObject.Technician;

namespace Repositories
{
    public interface IJobRepository
    {
        // Basic CRUD operations
        Task<Job?> GetByIdAsync(Guid jobId);
        Task<Job> CreateAsync(Job job);
        Task<Job> UpdateAsync(Job job);
        Task<bool> DeleteAsync(Guid jobId);
        Task<bool> ExistsAsync(Guid jobId);

        // Job-specific queries
        Task<IEnumerable<Job>> GetJobsByRepairOrderIdAsync(Guid repairOrderId);
        Task<IEnumerable<Job>> GetJobsByServiceIdAsync(Guid serviceId);
        Task<IEnumerable<Job>> GetJobsByStatusAsync(JobStatus status);
        Task<Job?> GetJobWithFullDetailsAsync(Guid jobId);
        Task<IEnumerable<Job>> GetPendingJobsByRepairOrderIdAsync(Guid repairOrderId);

        // Status management
        Task<bool> UpdateJobStatusAsync(Guid jobId, JobStatus newStatus, string? changeNote = null);
        Task<IEnumerable<Job>> BatchUpdateStatusAsync(List<(Guid JobId, JobStatus NewStatus, string? ChangeNote)> updates);

        // Job assignment and technician management
        Task<IEnumerable<Job>> GetJobsByTechnicianIdAsync(Guid technicianId);
        Task<bool> AssignTechnicianToJobAsync(Guid jobId, Guid technicianId);
        Task<bool> RemoveTechnicianFromJobAsync(Guid jobId, Guid technicianId);
        Task<IEnumerable<Job>> GetUnassignedJobsAsync();

        // Job parts management
        Task<IEnumerable<JobPart>> GetJobPartsAsync(Guid jobId);
        Task<bool> AddJobPartAsync(JobPart jobPart);
        Task<bool> UpdateJobPartAsync(JobPart jobPart);
        Task<bool> RemoveJobPartAsync(Guid jobPartId);
        Task<decimal> CalculateJobTotalAmountAsync(Guid jobId);

        // Repair activities management
        Task<IEnumerable<Repair>> GetJobRepairsAsync(Guid jobId);
        Task<Repair?> GetActiveRepairForJobAsync(Guid jobId);
        Task<bool> StartRepairForJobAsync(Guid jobId, Repair repair);
        Task<bool> CompleteRepairForJobAsync(Guid repairId, string? notes = null);

        // Filtering and search
        Task<IEnumerable<Job>> SearchJobsAsync(
            string? searchText = null,
            List<JobStatus>? statuses = null,
            List<Guid>? repairOrderIds = null,
            List<Guid>? serviceIds = null,
            List<Guid>? technicianIds = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        // Performance optimized queries
        Task<IEnumerable<Job>> GetJobsWithNavigationPropertiesAsync(
            Expression<Func<Job, bool>>? predicate = null,
            params Expression<Func<Job, object>>[] includes);

        // Statistics and reporting
        Task<Dictionary<JobStatus, int>> GetJobCountsByStatusAsync(List<Guid>? repairOrderIds = null);
        Task<Dictionary<string, object>> GetJobStatisticsAsync(Guid? repairOrderId = null);
        Task<IEnumerable<Job>> GetOverdueJobsAsync();
        Task<IEnumerable<Job>> GetJobsDueWithinDaysAsync(int days);

        // Business logic validation
        Task<bool> CanCompleteJobAsync(Guid jobId);
        Task<bool> CanStartJobAsync(Guid jobId);
        Task<bool> HasActiveTechnicianAsync(Guid jobId);

        // Audit and history
        Task<IEnumerable<Job>> GetRecentlyUpdatedJobsAsync(int hours = 24);
        Task<DateTime?> GetLastStatusChangeAsync(Guid jobId);

        // Level and priority management
        Task<IEnumerable<Job>> GetJobsByLevelAsync(int level);
        Task<IEnumerable<Job>> GetHighPriorityJobsAsync(int minLevel = 5);
        Task<bool> UpdateJobLevelAsync(Guid jobId, int newLevel);

        // Completion tracking
        Task<bool> MarkJobAsCompletedAsync(Guid jobId, string? completionNotes = null);
        Task<bool> MarkJobAsInProgressAsync(Guid jobId, Guid technicianId);
        Task<TimeSpan?> GetJobDurationAsync(Guid jobId);
        Task<decimal> GetJobProgressPercentageAsync(Guid jobId);

        // Customer approval workflow
        Task<bool> SendJobsToCustomerForApprovalAsync(List<Guid> jobIds, string managerId);
        Task<bool> ProcessCustomerApprovalAsync(Guid jobId, bool isApproved, string? customerNote = null);
        Task<IEnumerable<Job>> GetJobsWaitingCustomerApprovalAsync(Guid repairOrderId);
        Task<IEnumerable<Job>> GetJobsApprovedByCustomerAsync(Guid? repairOrderId = null);
        Task<IEnumerable<Job>> GetJobsRejectedByCustomerAsync(Guid? repairOrderId = null);
        
        // Manager assignment workflow
        Task<bool> AssignJobsToTechnicianAsync(List<Guid> jobIds, Guid technicianId, string managerId);
        Task<bool> ReassignJobToTechnicianAsync(Guid jobId, Guid newTechnicianId, string managerId);
        Task<IEnumerable<Job>> GetJobsReadyForAssignmentAsync(Guid? repairOrderId = null);
        Task<IEnumerable<Job>> GetJobsAssignedByManagerAsync(string managerId);
        
        // Bulk operations for RepairOrder workflow
        Task<Dictionary<JobStatus, int>> GetJobStatusCountsByRepairOrderAsync(Guid repairOrderId);
        
        // Estimate expiration and revision management
        Task<bool> SetJobEstimateExpirationAsync(Guid jobId, int expirationDays);
        Task<IEnumerable<Job>> GetExpiredEstimatesAsync();
        Task<bool> IsEstimateExpiredAsync(Guid jobId);
        Task<Job> ReviseJobEstimateAsync(Guid originalJobId, string managerId, string revisionReason);
        Task<IEnumerable<Job>> GetJobRevisionsAsync(Guid originalJobId);
        Task<Job?> GetLatestJobRevisionAsync(Guid originalJobId);
        Task<bool> ExpireOldEstimatesAsync();
    }
}