
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;
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
                .Include(j => j.Repair)
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
            try
            {
                await _context.SaveChangesAsync();
                return job;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error creating job: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to create job: {ex.Message}", ex);
            }
        }

        public async Task<Job> UpdateAsync(Job job)
        {
            job.UpdatedAt = DateTime.UtcNow;
            _context.Jobs.Update(job);
            try
            {
                await _context.SaveChangesAsync();
                return job;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error updating job: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to update job: {ex.Message}", ex);
            }
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

        #region Service Methods

        public async Task<Service?> GetServiceByIdAsync(Guid serviceId)
        {
            return await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId);
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
                .OrderBy(j => j.CreatedAt)
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
                .Include(j => j.Repair)
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
        public async Task<string> GetUserIdByTechnicianIdAsync(Guid technicianId)
        {
            var technician = await _context.Technicians
                .Where(t => t.TechnicianId == technicianId)
                .Select(t => t.UserId)
                .FirstOrDefaultAsync();

            return technician;
        }
        public async Task<Job?> GetJobByIdAsync(Guid jobId)
        {
            return await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.Brand)
                .Include(j => j.RepairOrder)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.Model)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .FirstOrDefaultAsync(j => j.JobId == jobId);
        }
        public async Task<bool> AssignTechnicianToJobAsync(Guid jobId, Guid technicianId)
        {
            // Check if assignment already exists
            var existingAssignment = await _context.JobTechnicians
                .FirstOrDefaultAsync(jt => jt.JobId == jobId && jt.TechnicianId == technicianId);

            if (existingAssignment != null) return true;

            var jobTechnician = new JobTechnician
            {
                JobId = jobId,
                TechnicianId = technicianId
            };

            _context.JobTechnicians.Add(jobTechnician);

            // Update the job status to New when assigned
            var job = await _context.Jobs.FindAsync(jobId);
            if (job != null)
            {
                job.Status = JobStatus.New;
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
                .OrderBy(j => j.CreatedAt)
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
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error adding job part: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to add job part {jobPart.PartId} to job {jobPart.JobId}: {ex.Message}", ex);
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
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error updating job part: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to update job part {jobPart.JobPartId}: {ex.Message}", ex);
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
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error removing job part: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to remove job part {jobPartId}: {ex.Message}", ex);
            }
        }

        public async Task<decimal> CalculateJobTotalAmountAsync(Guid jobId)
        {
            try
            {
                Console.WriteLine($"Calculating total amount for job ID: {jobId}");
                var job = await _context.Jobs
                    .Include(j => j.Service)
                    .Include(j => j.JobParts)
                    .FirstOrDefaultAsync(j => j.JobId == jobId);

                if (job == null) 
                {
                    Console.WriteLine($"Job not found for ID: {jobId}");
                    return 0;
                }

                var serviceAmount = job.Service?.Price ?? 0;
                var partsAmount = job.JobParts?.Sum(jp => jp.Quantity * jp.UnitPrice) ?? 0;
                
                Console.WriteLine($"Job service amount: {serviceAmount}, parts amount: {partsAmount}");

                return serviceAmount + partsAmount;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error calculating job total amount: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to calculate job total amount for job {jobId}: {ex.Message}", ex);
            }
        }

        private async Task UpdateJobTotalAmountAsync(Guid jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job != null)
            {
                job.TotalAmount = await CalculateJobTotalAmountAsync(jobId);
                job.UpdatedAt = DateTime.UtcNow;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log the exception for debugging purposes
                    Console.WriteLine($"Error updating job total amount: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw new InvalidOperationException($"Failed to update job total amount for job {jobId}: {ex.Message}", ex);
                }
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

       

       

        
        
        #endregion

        #region Manager Assignment Workflow

        public async Task<IEnumerable<Job>> GetJobsReadyForAssignmentAsync(Guid? repairOrderId = null)
        {
            var query = _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.RepairOrder)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .Where(j => j.Status == JobStatus.Pending || j.Status == JobStatus.New);

            if (repairOrderId.HasValue)
            {
                query = query.Where(j => j.RepairOrderId == repairOrderId.Value);
            }

            return await query
                .OrderBy(j => j.AssignedAt) // Changed from CustomerResponseAt to AssignedAt
                .ToListAsync();
        }

        public async Task<bool> TechnicianExistsAsync(Guid technicianId)
        {
            return await _context.Technicians.AnyAsync(t => t.TechnicianId == technicianId);
        }

        public async Task<IEnumerable<Technician>> GetTechniciansByBranchIdAsync(Guid branchId)
        {
            return await _context.Technicians
                .Include(t => t.User)
                .Where(t => t.User.BranchId == branchId)
                .ToListAsync();
        }

        public async Task<Technician?> GetTechnicianByUserIdAsync(string userId)
        {
            return await _context.Technicians
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task<Technician?> GetTechnicianByIdAsync(Guid technicianId)
        {
            return await _context.Technicians
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TechnicianId == technicianId);
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
                .Where(j => jobIds.Contains(j.JobId) && (j.Status == JobStatus.Pending || j.Status == JobStatus.New))
                .ToListAsync();

            if (!jobs.Any()) return false;

            var timestamp = DateTime.UtcNow;

            foreach (var job in jobs)
            {
                // Update job status from Pending to New when assigned
                job.Status = JobStatus.New;
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

            var timestamp = DateTime.UtcNow;

            // Remove all current technician assignments
            var currentAssignments = await _context.JobTechnicians
                .Where(jt => jt.JobId == jobId)
                .ToListAsync();

            if (currentAssignments.Any())
            {
                _context.JobTechnicians.RemoveRange(currentAssignments);
            }

            // Add new technician assignment
            var jobTechnician = new JobTechnician
            {
                JobId = jobId,
                TechnicianId = newTechnicianId
            };
            _context.JobTechnicians.Add(jobTechnician);

            // Update job assignment metadata
            job.AssignedByManagerId = managerId;
            job.AssignedAt = timestamp;
            job.UpdatedAt = timestamp;
            
            // Update job status to New when reassigned
            job.Status = JobStatus.New;

            // Do not add note about reassignment (as per requirement)
            // var noteText = $"[{timestamp:yyyy-MM-dd HH:mm:ss}] Job reassigned to technician {newTechnicianId} by manager {managerId}, status changed to New";
            // job.Note = string.IsNullOrEmpty(job.Note) ? noteText : $"{job.Note}\n{noteText}";

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
        
        // NEW: Create revision job
        public async Task<Job> CreateRevisionJobAsync(Guid originalJobId, string revisionReason)
        {
            // Get the original job
            var originalJob = await GetByIdAsync(originalJobId);
            if (originalJob == null)
                throw new ArgumentException("Original job not found", nameof(originalJobId));

            // Create a new job based on the original job
            var revisionJob = new Job
            {
                ServiceId = originalJob.ServiceId,
                RepairOrderId = originalJob.RepairOrderId,
                JobName = $"{originalJob.JobName} (Revision {originalJob.RevisionCount + 1})",
                Status = JobStatus.Pending,
                Deadline = originalJob.Deadline,
                TotalAmount = originalJob.TotalAmount,
                Note = $"Revision of job {originalJob.JobId}. Reason: {revisionReason}",
                AssignedByManagerId = null, // Reset assignment
                AssignedAt = null, // Reset assignment
                RevisionCount = originalJob.RevisionCount + 1,
                OriginalJobId = originalJob.OriginalJobId ?? originalJobId, // Point to the very first job
                RevisionReason = revisionReason,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            // Save the revision job
            var createdJob = await CreateAsync(revisionJob);

            // Copy job parts from the original job
            if (originalJob.JobParts != null && originalJob.JobParts.Any())
            {
                foreach (var originalPart in originalJob.JobParts)
                {
                    var jobPart = new JobPart
                    {
                        JobId = createdJob.JobId,
                        PartId = originalPart.PartId,
                        Quantity = originalPart.Quantity,
                        UnitPrice = originalPart.UnitPrice,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.JobParts.Add(jobPart);
                }
                
                await _context.SaveChangesAsync();
            }

            return createdJob;
        }
        
        // Create job with parts in a transaction
        public async Task<Job> CreateJobWithPartsAsync(Job job, List<JobPart> jobParts)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Console.WriteLine($"Creating job with ServiceId: {job.ServiceId}, RepairOrderId: {job.RepairOrderId}");
                
                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Created job with ID: {job.JobId}");
                
                if (jobParts != null && jobParts.Any())
                {
                    Console.WriteLine($"Adding {jobParts.Count} parts to job");
                    foreach (var jobPart in jobParts)
                    {
                        jobPart.JobId = job.JobId;
                        _context.JobParts.Add(jobPart);
                        Console.WriteLine($"Added part with PartId: {jobPart.PartId}, Quantity: {jobPart.Quantity}, UnitPrice: {jobPart.UnitPrice}");
                    }
                    await _context.SaveChangesAsync();
                }
                
                // Calculate total amount
                var totalAmount = await CalculateJobTotalAmountAsync(job.JobId);
                job.TotalAmount = totalAmount;
                _context.Jobs.Update(job);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Job total amount calculated: {totalAmount}");
                
                await transaction.CommitAsync();
                return job;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating job with parts: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        #endregion

    }
}