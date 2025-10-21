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

            var oldStatus = job.Status;
            job.Status = newStatus;
            job.UpdatedAt = DateTime.UtcNow;

            // Add change note to the job notes if provided
            if (!string.IsNullOrEmpty(changeNote))
            {
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                var statusChangeNote = $"[{timestamp}] Status changed from {oldStatus} to {newStatus}: {changeNote}";
                job.Note = string.IsNullOrEmpty(job.Note) 
                    ? statusChangeNote 
                    : $"{job.Note}\n{statusChangeNote}";
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

        public async Task<IEnumerable<Job>> BatchUpdateStatusAsync(List<(Guid JobId, JobStatus NewStatus, string? ChangeNote)> updates)
        {
            if (updates == null || !updates.Any()) return new List<Job>();

            var jobIds = updates.Select(u => u.JobId).ToList();
            var jobs = await _context.Jobs
                .Where(j => jobIds.Contains(j.JobId))
                .ToListAsync();

            var updatedJobs = new List<Job>();

            foreach (var update in updates)
            {
                var job = jobs.FirstOrDefault(j => j.JobId == update.JobId);
                if (job != null)
                {
                    var oldStatus = job.Status;
                    job.Status = update.NewStatus;
                    job.UpdatedAt = DateTime.UtcNow;

                    if (!string.IsNullOrEmpty(update.ChangeNote))
                    {
                        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                        var statusChangeNote = $"[{timestamp}] Batch update from {oldStatus} to {update.NewStatus}: {update.ChangeNote}";
                        job.Note = string.IsNullOrEmpty(job.Note) 
                            ? statusChangeNote 
                            : $"{job.Note}\n{statusChangeNote}";
                    }

                    updatedJobs.Add(job);
                }
            }

            if (updatedJobs.Any())
            {
                await _context.SaveChangesAsync();
            }

            return updatedJobs;
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

        #region Repair Activities Management

        public async Task<IEnumerable<Repair>> GetJobRepairsAsync(Guid jobId)
        {
            return await _context.Repairs
                .Where(r => r.JobId == jobId)
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();
        }

        public async Task<Repair?> GetActiveRepairForJobAsync(Guid jobId)
        {
            //return await _context.Repairs
            //    .FirstOrDefaultAsync(r => r.JobId == jobId && r.Status == RepairStatus.InProgress);

            return await _context.Repairs
                .FirstOrDefaultAsync(r => r.JobId == jobId );
        }

        public async Task<bool> StartRepairForJobAsync(Guid jobId, Repair repair)
        {
            // Ensure no other active repair for this job
            var activeRepair = await GetActiveRepairForJobAsync(jobId);
            if (activeRepair != null) return false;

            repair.JobId = jobId;
            //repair.Status = RepairStatus.InProgress;
            repair.StartTime = DateTime.UtcNow;

            _context.Repairs.Add(repair);

            // Update job status to InProgress
            var job = await _context.Jobs.FindAsync(jobId);
            if (job != null && job.Status == JobStatus.Pending)
            {
                job.Status = JobStatus.InProgress;
                job.UpdatedAt = DateTime.UtcNow;
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

        public async Task<bool> CompleteRepairForJobAsync(Guid repairId, string? notes = null)
        {
            var repair = await _context.Repairs.FindAsync(repairId);
            if (repair == null) return false;

            repair.Status = RepairStatus.Completed;
            repair.EndTime = DateTime.UtcNow;
            
            if (repair.StartTime.HasValue && repair.EndTime.HasValue)
            {
                repair.ActualTime = (long)(repair.EndTime.Value - repair.StartTime.Value).TotalMinutes;
            }

            if (!string.IsNullOrEmpty(notes))
            {
                repair.Notes = string.IsNullOrEmpty(repair.Notes) ? notes : $"{repair.Notes}\n{notes}";
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

        #endregion

        #region Statistics and Reporting

        public async Task<Dictionary<JobStatus, int>> GetJobCountsByStatusAsync(List<Guid>? repairOrderIds = null)
        {
            var query = _context.Jobs.AsQueryable();

            if (repairOrderIds != null && repairOrderIds.Any())
            {
                query = query.Where(j => repairOrderIds.Contains(j.RepairOrderId));
            }

            return await query
                .GroupBy(j => j.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, object>> GetJobStatisticsAsync(Guid? repairOrderId = null)
        {
            var query = _context.Jobs.AsQueryable();

            if (repairOrderId.HasValue)
            {
                query = query.Where(j => j.RepairOrderId == repairOrderId.Value);
            }

            var totalJobs = await query.CountAsync();
            var completedJobs = await query.CountAsync(j => j.Status == JobStatus.Completed);
            var inProgressJobs = await query.CountAsync(j => j.Status == JobStatus.InProgress);
            var pendingJobs = await query.CountAsync(j => j.Status == JobStatus.Pending);
            var overdueJobs = await query.CountAsync(j => j.Deadline.HasValue && j.Deadline.Value < DateTime.UtcNow && j.Status != JobStatus.Completed);
            
            var totalValue = await query.SumAsync(j => (decimal?)j.TotalAmount) ?? 0;
            var avgJobValue = totalJobs > 0 ? totalValue / totalJobs : 0;

            return new Dictionary<string, object>
            {
                ["TotalJobs"] = totalJobs,
                ["CompletedJobs"] = completedJobs,
                ["InProgressJobs"] = inProgressJobs,
                ["PendingJobs"] = pendingJobs,
                ["OverdueJobs"] = overdueJobs,
                ["TotalValue"] = totalValue,
                ["AverageJobValue"] = avgJobValue,
                ["CompletionRate"] = totalJobs > 0 ? (double)completedJobs / totalJobs * 100 : 0
            };
        }

        public async Task<IEnumerable<Job>> GetOverdueJobsAsync()
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .Where(j => j.Deadline.HasValue && 
                           j.Deadline.Value < DateTime.UtcNow && 
                           j.Status != JobStatus.Completed)
                .OrderBy(j => j.Deadline)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetJobsDueWithinDaysAsync(int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(days);

            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .Where(j => j.Deadline.HasValue && 
                           j.Deadline.Value <= cutoffDate && 
                           j.Deadline.Value >= DateTime.UtcNow &&
                           j.Status != JobStatus.Completed)
                .OrderBy(j => j.Deadline)
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

        #region Level and Priority Management

        public async Task<IEnumerable<Job>> GetJobsByLevelAsync(int level)
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .Where(j => j.Level == level)
                .OrderBy(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetHighPriorityJobsAsync(int minLevel = 5)
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .Where(j => j.Level >= minLevel)
                .OrderByDescending(j => j.Level)
                .ThenBy(j => j.Deadline ?? DateTime.MaxValue)
                .ToListAsync();
        }

        public async Task<bool> UpdateJobLevelAsync(Guid jobId, int newLevel)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null) return false;

            job.Level = newLevel;
            job.UpdatedAt = DateTime.UtcNow;

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

        #region Customer Approval Workflow

        public async Task<bool> SendJobsToCustomerForApprovalAsync(List<Guid> jobIds, string managerId)
        {
            if (jobIds == null || !jobIds.Any()) return false;

            var jobs = await _context.Jobs
                .Where(j => jobIds.Contains(j.JobId) && j.Status == JobStatus.Pending)
                .ToListAsync();

            if (!jobs.Any()) return false;

            var timestamp = DateTime.UtcNow;
            const int defaultExpirationDays = 7; // Default 7 days expiration

            foreach (var job in jobs)
            {
                job.Status = JobStatus.WaitingCustomerApproval;
                job.SentToCustomerAt = timestamp;
                job.EstimateExpiresAt = timestamp.AddDays(defaultExpirationDays); // Set expiration
                job.UpdatedAt = timestamp;
                
                // Add note about sending to customer
                var noteText = $"[{timestamp:yyyy-MM-dd HH:mm:ss}] Job sent to customer for approval by manager {managerId}. Expires: {job.EstimateExpiresAt:yyyy-MM-dd}";
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

        public async Task<bool> ProcessCustomerApprovalAsync(Guid jobId, bool isApproved, string? customerNote = null)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null || job.Status != JobStatus.WaitingCustomerApproval) return false;

            var timestamp = DateTime.UtcNow;
            job.Status = isApproved ? JobStatus.CustomerApproved : JobStatus.CustomerRejected;
            job.CustomerResponseAt = timestamp;
            job.CustomerApprovalNote = customerNote;
            job.UpdatedAt = timestamp;

            // Add note about customer response
            var responseText = isApproved ? "approved" : "rejected";
            var noteText = $"[{timestamp:yyyy-MM-dd HH:mm:ss}] Customer {responseText} the job";
            if (!string.IsNullOrEmpty(customerNote))
            {
                noteText += $": {customerNote}";
            }
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

        public async Task<IEnumerable<Job>> GetJobsWaitingCustomerApprovalAsync(Guid repairOrderId)
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .Where(j => j.RepairOrderId == repairOrderId && j.Status == JobStatus.WaitingCustomerApproval)
                .OrderBy(j => j.SentToCustomerAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetJobsApprovedByCustomerAsync(Guid? repairOrderId = null)
        {
            var query = _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .Where(j => j.Status == JobStatus.CustomerApproved);

            if (repairOrderId.HasValue)
            {
                query = query.Where(j => j.RepairOrderId == repairOrderId.Value);
            }

            return await query
                .OrderBy(j => j.CustomerResponseAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetJobsRejectedByCustomerAsync(Guid? repairOrderId = null)
        {
            var query = _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .Where(j => j.Status == JobStatus.CustomerRejected);

            if (repairOrderId.HasValue)
            {
                query = query.Where(j => j.RepairOrderId == repairOrderId.Value);
            }

            return await query
                .OrderByDescending(j => j.CustomerResponseAt)
                .ToListAsync();
        }

        #endregion

        #region Manager Assignment Workflow

        public async Task<bool> AssignJobsToTechnicianAsync(List<Guid> jobIds, Guid technicianId, string managerId)
        {
            if (jobIds == null || !jobIds.Any()) return false;

            var jobs = await _context.Jobs
                .Where(j => jobIds.Contains(j.JobId) && j.Status == JobStatus.CustomerApproved)
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

        public async Task<IEnumerable<Job>> GetJobsReadyForAssignmentAsync(Guid? repairOrderId = null)
        {
            var query = _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .Where(j => j.Status == JobStatus.CustomerApproved);

            if (repairOrderId.HasValue)
            {
                query = query.Where(j => j.RepairOrderId == repairOrderId.Value);
            }

            return await query
                .OrderBy(j => j.CustomerResponseAt)
                .ToListAsync();
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

        #endregion

        #region Bulk Operations for RepairOrder Workflow


        public async Task<Dictionary<JobStatus, int>> GetJobStatusCountsByRepairOrderAsync(Guid repairOrderId)
        {
            return await _context.Jobs
                .Where(j => j.RepairOrderId == repairOrderId)
                .GroupBy(j => j.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        #endregion
        
        #region Estimate Expiration and Revision Management

        public async Task<bool> SetJobEstimateExpirationAsync(Guid jobId, int expirationDays)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null) return false;

            // Only set expiration when sending to customer
            if (job.SentToCustomerAt.HasValue)
            {
                job.EstimateExpiresAt = job.SentToCustomerAt.Value.AddDays(expirationDays);
                job.UpdatedAt = DateTime.UtcNow;
                
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
            
            return false;
        }

        public async Task<IEnumerable<Job>> GetExpiredEstimatesAsync()
        {
            var now = DateTime.UtcNow;
            
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                    .ThenInclude(ro => ro.User)
                .Where(j => j.Status == JobStatus.WaitingCustomerApproval && 
                           j.EstimateExpiresAt.HasValue && 
                           j.EstimateExpiresAt.Value < now)
                .OrderBy(j => j.EstimateExpiresAt)
                .ToListAsync();
        }

        public async Task<bool> IsEstimateExpiredAsync(Guid jobId)
        {
            var job = await _context.Jobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.JobId == jobId);
            
            if (job == null || !job.EstimateExpiresAt.HasValue) return false;
            
            return job.EstimateExpiresAt.Value < DateTime.UtcNow;
        }

        public async Task<Job> ReviseJobEstimateAsync(Guid originalJobId, string managerId, string revisionReason)
        {
            var originalJob = await _context.Jobs
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .FirstOrDefaultAsync(j => j.JobId == originalJobId);
            
            if (originalJob == null)
                throw new ArgumentException("Original job not found", nameof(originalJobId));

            // Can only revise jobs that are waiting for approval or rejected
            if (originalJob.Status != JobStatus.WaitingCustomerApproval && 
                originalJob.Status != JobStatus.CustomerRejected)
            {
                throw new InvalidOperationException("Job cannot be revised in current status");
            }

            // Create revised job
            var revisedJob = new Job
            {
                ServiceId = originalJob.ServiceId,
                RepairOrderId = originalJob.RepairOrderId,
                JobName = originalJob.JobName,
                Status = JobStatus.Pending,
                Deadline = originalJob.Deadline,
                TotalAmount = originalJob.TotalAmount,
                Note = originalJob.Note,
                Level = originalJob.Level,
                OriginalJobId = originalJob.OriginalJobId ?? originalJobId, // Link to original or keep existing link
                RevisionCount = originalJob.RevisionCount + 1,
                RevisionReason = revisionReason,
                CreatedAt = DateTime.UtcNow
            };

            _context.Jobs.Add(revisedJob);
            await _context.SaveChangesAsync();

            // Copy job parts from original
            foreach (var originalPart in originalJob.JobParts)
            {
                var revisedPart = new JobPart
                {
                    JobId = revisedJob.JobId,
                    PartId = originalPart.PartId,
                    Quantity = originalPart.Quantity,
                    UnitPrice = originalPart.UnitPrice,
                    CreatedAt = DateTime.UtcNow
                };
                _context.JobParts.Add(revisedPart);
            }

            // Mark original job as superseded
            originalJob.Status = JobStatus.CustomerRejected;
            originalJob.UpdatedAt = DateTime.UtcNow;
            
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            var revisionNote = $"[{timestamp}] Job revised by manager {managerId}. Reason: {revisionReason}";
            originalJob.Note = string.IsNullOrEmpty(originalJob.Note) 
                ? revisionNote 
                : $"{originalJob.Note}\n{revisionNote}";

            await _context.SaveChangesAsync();
            
            return revisedJob;
        }

        public async Task<IEnumerable<Job>> GetJobRevisionsAsync(Guid originalJobId)
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .Where(j => j.OriginalJobId == originalJobId || j.JobId == originalJobId)
                .OrderBy(j => j.RevisionCount)
                .ThenBy(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<Job?> GetLatestJobRevisionAsync(Guid originalJobId)
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .Where(j => j.OriginalJobId == originalJobId || j.JobId == originalJobId)
                .OrderByDescending(j => j.RevisionCount)
                .ThenByDescending(j => j.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExpireOldEstimatesAsync()
        {
            var expiredJobs = await GetExpiredEstimatesAsync();
            
            if (!expiredJobs.Any()) return true;

            var timestamp = DateTime.UtcNow;
            
            foreach (var job in expiredJobs)
            {
                job.Status = JobStatus.CustomerRejected;
                job.UpdatedAt = timestamp;
                
                var expireNote = $"[{timestamp:yyyy-MM-dd HH:mm:ss}] Estimate expired automatically";
                job.Note = string.IsNullOrEmpty(job.Note) 
                    ? expireNote 
                    : $"{job.Note}\n{expireNote}";
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
    }
}