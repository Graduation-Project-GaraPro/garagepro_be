using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
//using BusinessObject.InspectionAndRepair;
using BusinessObject.Technician;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly MyAppDbContext _context;

        public JobRepository(MyAppDbContext context)
        {
            _context = context;
        }

        #region Basic CRUD Operations

        public async Task<Job?> GetByIdAsync(Guid jobId)
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .Include(j => j.Repairs)
                .FirstOrDefaultAsync(j => j.JobId == jobId);
        }

        public async Task<IEnumerable<Job>> GetAllAsync()
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .ToListAsync();
        }

        public async Task<Job> CreateAsync(Job job)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<Job> UpdateAsync(Job job)
        {
            job.UpdatedAt = DateTime.UtcNow;
            _context.Jobs.Update(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<bool> DeleteAsync(Guid jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null) return false;

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid jobId)
        {
            return await _context.Jobs.AnyAsync(j => j.JobId == jobId);
        }

        #endregion

        #region Job-specific Queries

        public async Task<IEnumerable<Job>> GetJobsByRepairOrderIdAsync(Guid repairOrderId)
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .Where(j => j.RepairOrderId == repairOrderId)
                .OrderBy(j => j.Level)
                .ThenBy(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetJobsByServiceIdAsync(Guid serviceId)
        {
            return await _context.Jobs
                .Include(j => j.RepairOrder)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .Where(j => j.ServiceId == serviceId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetJobsByStatusAsync(JobStatus status)
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .Where(j => j.Status == status)
                .OrderBy(j => j.Deadline ?? DateTime.MaxValue)
                .ThenBy(j => j.Level)
                .ToListAsync();
        }

        public async Task<Job?> GetJobWithFullDetailsAsync(Guid jobId)
        {
            return await _context.Jobs
                .Include(j => j.Service)
                    .ThenInclude(s => s.ServiceCategory)
                .Include(j => j.RepairOrder)
                    .ThenInclude(ro => ro.Vehicle)
                .Include(j => j.RepairOrder)
                    .ThenInclude(ro => ro.User)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                        .ThenInclude(p => p.PartCategory)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .Include(j => j.Repairs)
                .FirstOrDefaultAsync(j => j.JobId == jobId);
        }

        public async Task<IEnumerable<Job>> GetPendingJobsByRepairOrderIdAsync(Guid repairOrderId)
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .Where(j => j.RepairOrderId == repairOrderId && j.Status == JobStatus.Pending)
                .OrderBy(j => j.CreatedAt)
                .ToListAsync();
        }

        #endregion

        #region Status Management

        public async Task<bool> UpdateJobStatusAsync(Guid jobId, JobStatus newStatus, string? changeNote = null)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null) return false;

            job.Status = newStatus;
            job.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(changeNote))
            {
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                var noteText = $"[{timestamp}] Status changed to {newStatus}: {changeNote}";
                job.Note = string.IsNullOrEmpty(job.Note) ? noteText : $"{job.Note}\n{noteText}";
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> BatchUpdateStatusAsync(List<(Guid JobId, JobStatus NewStatus, string? ChangeNote)> updates)
        {
            if (updates == null || !updates.Any()) return true;

            var jobIds = updates.Select(u => u.JobId).ToList();
            var jobs = await _context.Jobs
                .Where(j => jobIds.Contains(j.JobId))
                .ToListAsync();

            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            foreach (var update in updates)
            {
                var job = jobs.FirstOrDefault(j => j.JobId == update.JobId);
                if (job == null) continue;

                job.Status = update.NewStatus;
                job.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(update.ChangeNote))
                {
                    var noteText = $"[{timestamp}] Status changed to {update.NewStatus}: {update.ChangeNote}";
                    job.Note = string.IsNullOrEmpty(job.Note) ? noteText : $"{job.Note}\n{noteText}";
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Job Assignment and Technician Management

        public async Task<IEnumerable<Job>> GetJobsByTechnicianIdAsync(Guid technicianId)
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .Where(j => j.JobTechnicians.Any(jt => jt.TechnicianId == technicianId))
                .OrderBy(j => j.Status)
                .ThenBy(j => j.Deadline ?? DateTime.MaxValue)
                .ToListAsync();
        }

        public async Task<bool> AssignTechnicianToJobAsync(Guid jobId, Guid technicianId)
        {
            // Check if assignment already exists
            var existingAssignment = await _context.JobTechnicians
                .FirstOrDefaultAsync(jt => jt.JobId == jobId && jt.TechnicianId == technicianId);

            if (existingAssignment != null) return true; // Already assigned

            var jobTechnician = new JobTechnician
            {
                JobId = jobId,
                TechnicianId = technicianId
            };

            _context.JobTechnicians.Add(jobTechnician);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveTechnicianFromJobAsync(Guid jobId, Guid technicianId)
        {
            var jobTechnician = await _context.JobTechnicians
                .FirstOrDefaultAsync(jt => jt.JobId == jobId && jt.TechnicianId == technicianId);

            if (jobTechnician == null) return false;

            _context.JobTechnicians.Remove(jobTechnician);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Job>> GetUnassignedJobsAsync()
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Where(j => !j.JobTechnicians.Any())
                .OrderBy(j => j.Level)
                .ThenBy(j => j.CreatedAt)
                .ToListAsync();
        }

        #endregion

        #region Job Parts Management

        public async Task<IEnumerable<JobPart>> GetJobPartsAsync(Guid jobId)
        {
            return await _context.JobParts
                .Include(jp => jp.Part)
                    .ThenInclude(p => p.PartCategory)
                .Where(jp => jp.JobId == jobId)
                .OrderBy(jp => jp.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> AddJobPartAsync(JobPart jobPart)
        {
            _context.JobParts.Add(jobPart);

            try
            {
                await _context.SaveChangesAsync();
                
                // Update job total amount
                await UpdateJobTotalAmountAsync(jobPart.JobId);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateJobPartAsync(JobPart jobPart)
        {
            jobPart.UpdatedAt = DateTime.UtcNow;
            _context.JobParts.Update(jobPart);

            try
            {
                await _context.SaveChangesAsync();
                
                // Update job total amount
                await UpdateJobTotalAmountAsync(jobPart.JobId);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveJobPartAsync(Guid jobPartId)
        {
            var jobPart = await _context.JobParts.FindAsync(jobPartId);
            if (jobPart == null) return false;

            var jobId = jobPart.JobId;
            _context.JobParts.Remove(jobPart);

            try
            {
                await _context.SaveChangesAsync();
                
                // Update job total amount
                await UpdateJobTotalAmountAsync(jobId);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<decimal> CalculateJobTotalAmountAsync(Guid jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.JobParts)
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null) return 0;

            var serviceAmount = job.Service?.Price ?? 0;
            var partsAmount = job.JobParts?.Sum(jp => jp.Quantity * jp.UnitPrice) ?? 0;

            return serviceAmount + partsAmount;
        }

        private async Task UpdateJobTotalAmountAsync(Guid jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job != null)
            {
                job.TotalAmount = await CalculateJobTotalAmountAsync(jobId);
                job.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Filtering and Search

        public async Task<IEnumerable<Job>> SearchJobsAsync(
            string? searchText = null,
            List<JobStatus>? statuses = null,
            List<Guid>? repairOrderIds = null,
            List<Guid>? serviceIds = null,
            List<Guid>? technicianIds = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchText))
            {
                var searchLower = searchText.ToLower();
                query = query.Where(j => 
                    (j.JobName != null && j.JobName.ToLower().Contains(searchLower)) ||
                    (j.Service.ServiceName != null && j.Service.ServiceName.ToLower().Contains(searchLower)) ||
                    (j.Note != null && j.Note.ToLower().Contains(searchLower)));
            }

            if (statuses != null && statuses.Any())
            {
                query = query.Where(j => statuses.Contains(j.Status));
            }

            if (repairOrderIds != null && repairOrderIds.Any())
            {
                query = query.Where(j => repairOrderIds.Contains(j.RepairOrderId));
            }

            if (serviceIds != null && serviceIds.Any())
            {
                query = query.Where(j => serviceIds.Contains(j.ServiceId));
            }

            if (technicianIds != null && technicianIds.Any())
            {
                query = query.Where(j => j.JobTechnicians.Any(jt => technicianIds.Contains(jt.TechnicianId)));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(j => j.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(j => j.CreatedAt <= toDate.Value);
            }

            return await query
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetJobsWithNavigationPropertiesAsync(
            Expression<Func<Job, bool>>? predicate = null,
            params Expression<Func<Job, object>>[] includes)
        {
            var query = _context.Jobs.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetJobsWithNavigationPropertiesAsync()
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .ToListAsync();
        }

        #endregion

        #region Business Logic Validation

        public async Task<bool> CanCompleteJobAsync(Guid jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.Repairs)
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null) return false;

            // Business rules for job completion
            // 1. Job must be in progress
            if (job.Status != JobStatus.InProgress) return false;

            // 2. All repairs should be completed
            var hasActiveRepairs = job.Repairs.Any(r => r.Status == RepairStatus.InProgress);
            if (hasActiveRepairs) return false;

            // 3. Must have at least one completed repair (optional business rule)
            var hasCompletedRepairs = job.Repairs.Any(r => r.Status == RepairStatus.Completed);
            
            return hasCompletedRepairs;
        }

        public async Task<bool> CanStartJobAsync(Guid jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.JobTechnicians)
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null) return false;

            // Business rules for starting a job
            // 1. Job must be assigned to technician (approved by customer and assigned by manager)
            if (job.Status != JobStatus.AssignedToTechnician) return false;

            // 2. Must have at least one assigned technician
            return job.JobTechnicians.Any();
        }

        public async Task<bool> HasActiveTechnicianAsync(Guid jobId)
        {
            return await _context.JobTechnicians
                .AnyAsync(jt => jt.JobId == jobId);
        }

        #endregion

        #region Audit and History

        public async Task<IEnumerable<Job>> GetRecentlyUpdatedJobsAsync(int hours = 24)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hours);

            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Where(j => j.UpdatedAt >= cutoffTime || j.CreatedAt >= cutoffTime)
                .OrderByDescending(j => j.UpdatedAt ?? j.CreatedAt)
                .ToListAsync();
        }

        public async Task<DateTime?> GetLastStatusChangeAsync(Guid jobId)
        {
            var job = await _context.Jobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            return job?.UpdatedAt ?? job?.CreatedAt;
        }

        #endregion

        #region Completion Tracking

        public async Task<bool> MarkJobAsCompletedAsync(Guid jobId, string? completionNotes = null)
        {
            if (!await CanCompleteJobAsync(jobId)) return false;

            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null) return false;

            job.Status = JobStatus.Completed;
            job.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(completionNotes))
            {
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                var completionNote = $"[{timestamp}] Job completed: {completionNotes}";
                job.Note = string.IsNullOrEmpty(job.Note) 
                    ? completionNote 
                    : $"{job.Note}\n{completionNote}";
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> MarkJobAsInProgressAsync(Guid jobId, Guid technicianId)
        {
            if (!await CanStartJobAsync(jobId)) return false;

            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null) return false;

            job.Status = JobStatus.InProgress;
            job.UpdatedAt = DateTime.UtcNow;

            // Ensure technician is assigned
            await AssignTechnicianToJobAsync(jobId, technicianId);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<TimeSpan?> GetJobDurationAsync(Guid jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.Repairs)
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null) return null;

            // Calculate duration based on repairs
            var totalMinutes = job.Repairs
                .Where(r => r.ActualTime.HasValue)
                .Sum(r => r.ActualTime.Value);

            return totalMinutes > 0 ? TimeSpan.FromMinutes(totalMinutes) : null;
        }

        public async Task<decimal> GetJobProgressPercentageAsync(Guid jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.Repairs)
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null) return 0;

            if (job.Status == JobStatus.Completed) return 100;
            if (job.Status == JobStatus.Pending) return 0;

            // Calculate progress based on repairs
            var totalRepairs = job.Repairs.Count;
            if (totalRepairs == 0) return job.Status == JobStatus.InProgress ? 50 : 0;

            var completedRepairs = job.Repairs.Count(r => r.Status == RepairStatus.Completed);
            
            return totalRepairs > 0 ? (decimal)completedRepairs / totalRepairs * 100 : 0;
        }

        #endregion

        #region Manager Assignment Workflow

        public async Task<IEnumerable<Job>> GetJobsReadyForAssignmentAsync(Guid? repairOrderId = null)
        {
            var query = _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .Where(j => j.Status == JobStatus.Pending);

            if (repairOrderId.HasValue)
            {
                query = query.Where(j => j.RepairOrderId == repairOrderId.Value);
            }

            return await query
                .OrderBy(j => j.CustomerResponseAt)
                .ToListAsync();
        }

        public async Task<bool> ReassignJobToTechnicianAsync(Guid jobId, Guid newTechnicianId, string managerId)
        {
            var job = await _context.Jobs
                .Include(j => j.JobTechnicians)
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null) return false;

            // Can only reassign jobs that are assigned or in progress
            if (job.Status != JobStatus.AssignedToTechnician && job.Status != JobStatus.InProgress) return false;

            var timestamp = DateTime.UtcNow;

            // Remove existing technician assignments
            var existingAssignments = job.JobTechnicians.ToList();
            foreach (var assignment in existingAssignments)
            {
                _context.JobTechnicians.Remove(assignment);
            }

            // Add new technician assignment
            var newAssignment = new JobTechnician
            {
                JobId = jobId,
                TechnicianId = newTechnicianId
            };
            _context.JobTechnicians.Add(newAssignment);

            // Update job status and tracking
            job.Status = JobStatus.AssignedToTechnician;
            job.AssignedByManagerId = managerId;
            job.AssignedAt = timestamp;
            job.UpdatedAt = timestamp;

            // Add note about reassignment
            var noteText = $"[{timestamp:yyyy-MM-dd HH:mm:ss}] Job reassigned to new technician by manager {managerId}";
            job.Note = string.IsNullOrEmpty(job.Note) ? noteText : $"{job.Note}\n{noteText}";

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Job>> GetJobsAssignedByManagerAsync(string managerId)
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .Where(j => j.AssignedByManagerId == managerId)
                .OrderByDescending(j => j.AssignedAt)
                .ToListAsync();
        }

        public async Task<bool> AssignJobsToTechnicianAsync(List<Guid> jobIds, Guid technicianId, string managerId)
        {
            if (jobIds == null || !jobIds.Any()) return false;

            var jobs = await _context.Jobs
                .Where(j => jobIds.Contains(j.JobId) && j.Status == JobStatus.Pending)
                .ToListAsync();

            if (!jobs.Any()) return false;

            var timestamp = DateTime.UtcNow;

            foreach (var job in jobs)
            {
                job.Status = JobStatus.AssignedToTechnician;
                job.AssignedByManagerId = managerId;
                job.AssignedAt = timestamp;
                job.UpdatedAt = timestamp;

                // Add technician assignment
                var existingAssignment = await _context.JobTechnicians
                    .FirstOrDefaultAsync(jt => jt.JobId == job.JobId && jt.TechnicianId == technicianId);

                if (existingAssignment == null)
                {
                    var jobTechnician = new JobTechnician
                    {
                        JobId = job.JobId,
                        TechnicianId = technicianId
                    };
                    _context.JobTechnicians.Add(jobTechnician);
                }

                // Add note about assignment
                var noteText = $"[{timestamp:yyyy-MM-dd HH:mm:ss}] Job assigned to technician by manager {managerId}";
                job.Note = string.IsNullOrEmpty(job.Note) ? noteText : $"{job.Note}\n{noteText}";
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Job>> GetJobsRejectedByCustomerAsync(Guid? repairOrderId = null)
        {
            // This method is no longer relevant as customer approval is handled at the quotation level
            // Return empty collection
            return new List<Job>();
        }

        #endregion

    }
}