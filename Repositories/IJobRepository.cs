using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;

using BusinessObject.InspectionAndRepair;
using System.Linq.Expressions;

namespace Repositories
{
    public interface IJobRepository
    {
        // Basic CRUD operations
        Task<Job?> GetByIdAsync(Guid jobId);
        Task<IEnumerable<Job>> GetAllAsync();
        Task<IEnumerable<Job>> GetJobsByRepairOrderIdAsync(Guid repairOrderId);
        Task<IEnumerable<Job>> GetJobsByStatusAsync(JobStatus status);
        Task<Job> CreateAsync(Job job);
        Task<Job> UpdateAsync(Job job);
        Task<bool> DeleteAsync(Guid jobId);
        Task<bool> ExistsAsync(Guid jobId);

        // Job assignment and technician management
        Task<bool> AssignTechnicianToJobAsync(Guid jobId, Guid technicianId);
        Task<bool> RemoveTechnicianFromJobAsync(Guid jobId, Guid technicianId);
        Task<IEnumerable<Job>> GetUnassignedJobsAsync();
        Task<IEnumerable<Job>> GetJobsByTechnicianIdAsync(Guid technicianId);
        Task<bool> AssignJobsToTechnicianAsync(List<Guid> jobIds, Guid technicianId, string managerId);
        Task<bool> ReassignJobToTechnicianAsync(Guid jobId, Guid newTechnicianId, string managerId);
        Task<IEnumerable<Job>> GetJobsReadyForAssignmentAsync(Guid? repairOrderId = null);
        
        // Technician methods
        Task<IEnumerable<Technician>> GetTechniciansByBranchIdAsync(Guid branchId);
        Task<Technician?> GetTechnicianByUserIdAsync(string userId);
        Task<bool> TechnicianExistsAsync(Guid technicianId);
        
        // Job parts management
        Task<IEnumerable<JobPart>> GetJobPartsAsync(Guid jobId);
        Task<bool> AddJobPartAsync(JobPart jobPart);
        Task<bool> UpdateJobPartAsync(JobPart jobPart);
        Task<bool> RemoveJobPartAsync(Guid jobPartId);
        Task<decimal> CalculateJobTotalAmountAsync(Guid jobId);

        // Status management
        Task<bool> UpdateJobStatusAsync(Guid jobId, JobStatus newStatus, string? changeNote = null);
        Task<bool> BatchUpdateStatusAsync(List<(Guid JobId, JobStatus NewStatus, string? ChangeNote)> updates);
        Task<DateTime?> GetLastStatusChangeAsync(Guid jobId);

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
        Task<IEnumerable<Job>> GetJobsWithNavigationPropertiesAsync();
        Task<IEnumerable<Job>> GetRecentlyUpdatedJobsAsync(int hours = 24);
        
        // NEW: Create revision job
        Task<Job> CreateRevisionJobAsync(Guid originalJobId, string revisionReason);

    }
}