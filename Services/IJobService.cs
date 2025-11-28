using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;

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

        // NEW: Get all jobs with OData support
        Task<IEnumerable<Job>> GetAllJobsAsync();
        
        // NEW: Get jobs by status
        Task<IEnumerable<Job>> GetJobsByStatusIdAsync(JobStatus status);

        // Job Queries by Context
        Task<IEnumerable<Job>> GetJobsByRepairOrderIdAsync(Guid repairOrderId);
        Task<IEnumerable<Job>> GetJobsByStatusAsync(JobStatus status);

        // Manager Assignment Workflow
        Task<bool> AssignJobsToTechnicianAsync(List<Guid> jobIds, Guid technicianId, string managerId);
       // Task<bool> ReassignJobToTechnicianAsync(Guid jobId, Guid newTechnicianId, string managerId);
        // NEW: Create revision jobs from existing jobs
        Task<Job> CreateRevisionJobAsync(Guid originalJobId, string revisionReason);

        // Job Parts Management
        Task<IEnumerable<JobPart>> GetJobPartsAsync(Guid jobId);
        Task<bool> AddJobPartAsync(JobPart jobPart);
        Task<bool> UpdateJobPartAsync(JobPart jobPart);
        Task<bool> RemoveJobPartAsync(Guid jobPartId);
        Task<decimal> CalculateJobTotalAmountAsync(Guid jobId);

        // Status Management
        Task<bool> UpdateJobStatusAsync(Guid jobId, JobStatus newStatus, string? changeNote = null);
        Task<bool> BatchUpdateStatusAsync(List<(Guid JobId, JobStatus NewStatus, string? ChangeNote)> updates);

        // Service Methods
        Task<Service?> GetServiceByIdAsync(Guid serviceId);

        // Business Logic Validation
        Task<bool> CanAssignJobToTechnicianAsync(Guid jobId);

        // Search and Filtering
        Task<IEnumerable<Job>> SearchJobsAsync(
            string? searchText = null,
            List<JobStatus>? statuses = null,
            List<Guid>? repairOrderIds = null,
            List<Guid>? serviceIds = null,
            List<Guid>? technicianIds = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        // Technician methods
        Task<IEnumerable<Technician>> GetTechniciansByBranchIdAsync(Guid branchId);
        Task<Technician?> GetTechnicianByUserIdAsync(string userId);

        // Workflow Validation
        Task<bool> ValidateJobWorkflowAsync(Guid jobId, JobStatus targetStatus);
        Task<string> GetNextAllowedStatusesAsync(Guid jobId);
        
        // Create job with parts in a transaction
        Task<Job> CreateJobWithPartsAsync(Job job, List<JobPart> jobParts);
        
    }
}