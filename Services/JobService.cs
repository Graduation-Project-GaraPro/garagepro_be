﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;
using Microsoft.AspNetCore.SignalR;
using Repositories;
using Services.Hubs;
using Services.Notifications;

namespace Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IHubContext<TechnicianAssignmentHub> _technicianAssignmentHubContext;
        private readonly IHubContext<JobHub> _jobHubContext;
        private readonly INotificationService _notificationService;

        public JobService(IJobRepository jobRepository, IHubContext<TechnicianAssignmentHub> technicianAssignmentHubContext, IHubContext<JobHub> jobHubContext, INotificationService notificationService)
        {
            _jobRepository = jobRepository;
            _technicianAssignmentHubContext = technicianAssignmentHubContext;
            _jobHubContext = jobHubContext;
            _notificationService = notificationService;
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

            try
            {
                return await _jobRepository.CreateAsync(job);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error creating job in service: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to create job: {ex.Message}", ex);
            }
        }

        public async Task<Job> UpdateJobAsync(Job job)
        {
            var existingJob = await _jobRepository.GetByIdAsync(job.JobId);
            if (existingJob == null)
                throw new ArgumentException("Job not found", nameof(job.JobId));

            // Preserve audit fields
            job.CreatedAt = existingJob.CreatedAt;
            job.UpdatedAt = DateTime.UtcNow;

            try
            {
                return await _jobRepository.UpdateAsync(job);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error updating job in service: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to update job: {ex.Message}", ex);
            }
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

            if (!await _jobRepository.TechnicianExistsAsync(technicianId))
                throw new InvalidOperationException($"Technician with ID {technicianId} not found.");

            foreach (var jobId in jobIds)
            {
                if (!await CanAssignJobToTechnicianAsync(jobId))
                    throw new InvalidOperationException($"Job {jobId} cannot be assigned");
            }

            var result = await _jobRepository.AssignJobsToTechnicianAsync(jobIds, technicianId, managerId);

            if (result)
            {
                // LẤY USERID TỪ TECHNICIANID
                var userId = await _jobRepository.GetUserIdByTechnicianIdAsync(technicianId);

                foreach (var jobId in jobIds)
                {
                    var job = await _jobRepository.GetJobByIdAsync(jobId);

                    if (job != null)
                    {
                        // Gửi SignalR JobHub (real-time job update)
                        await _jobHubContext.Clients
                            .Group($"Technician_{technicianId}")
                            .SendAsync("JobAssigned", new
                            {
                                JobId = jobId,
                                TechnicianId = technicianId,
                                JobName = job.JobName,
                                ServiceName = job.Service?.ServiceName,
                                RepairOrderId = job.RepairOrderId,
                                Status = job.Status.ToString(),
                                AssignedAt = DateTime.UtcNow,
                                Message = "You have been assigned a new job"
                            });

                        // GỬI NOTIFICATION (LƯU DB + SIGNALR)
                        if (!string.IsNullOrEmpty(userId))
                        {
                            await _notificationService.SendJobAssignedNotificationAsync(
                                userId,
                                jobId,
                                job.JobName,
                                job.Service?.ServiceName ?? "N/A"
                            );
                        }

                        Console.WriteLine($"[JobService] Job {jobId} assigned and notification sent to User {userId}");
                    }
                }
            }

            return result;
        }

        public async Task<bool> ReassignJobToTechnicianAsync(Guid jobId, Guid newTechnicianId, string managerId)
        {
            if (jobId == Guid.Empty)
                throw new ArgumentException("Job ID is required", nameof(jobId));

            if (newTechnicianId == Guid.Empty)
                throw new ArgumentException("Technician ID is required", nameof(newTechnicianId));

            if (string.IsNullOrWhiteSpace(managerId))
                throw new ArgumentException("Manager ID is required", nameof(managerId));

            if (!await _jobRepository.TechnicianExistsAsync(newTechnicianId))
                throw new InvalidOperationException($"Technician with ID {newTechnicianId} not found.");

            var result = await _jobRepository.ReassignJobToTechnicianAsync(jobId, newTechnicianId, managerId);

            if (result)
            {
                var job = await _jobRepository.GetJobByIdAsync(jobId);
                var userId = await _jobRepository.GetUserIdByTechnicianIdAsync(newTechnicianId);

                if (job != null)
                {
                    // SignalR JobHub
                    await _jobHubContext.Clients
                        .Group($"Technician_{newTechnicianId}")
                        .SendAsync("JobReassigned", new
                        {
                            JobId = jobId,
                            TechnicianId = newTechnicianId,
                            JobName = job.JobName,
                            ServiceName = job.Service?.ServiceName,
                            RepairOrderId = job.RepairOrderId,
                            Status = job.Status.ToString(),
                            ReassignedAt = DateTime.UtcNow,
                            Message = "A job has been reassigned to you"
                        });

                    // GỬI NOTIFICATION
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _notificationService.SendJobReassignedNotificationAsync(
                            userId,
                            jobId,
                            job.JobName,
                            job.Service?.ServiceName ?? "N/A"
                        );
                    }

                    Console.WriteLine($"[JobService] Job {jobId} reassigned to User {userId}");
                }
            }

            return result;
        }

        public async Task<IEnumerable<Technician>> GetTechniciansByBranchIdAsync(Guid branchId)
        {
            return await _jobRepository.GetTechniciansByBranchIdAsync(branchId);
        }

        public async Task<Technician?> GetTechnicianByUserIdAsync(string userId)
        {
            return await _jobRepository.GetTechnicianByUserIdAsync(userId);
        }
        
        public async Task<bool> TechnicianExistsAsync(Guid technicianId)
        {
            return await _jobRepository.TechnicianExistsAsync(technicianId);
        }
        
        // NEW: Create revision job
        public async Task<Job> CreateRevisionJobAsync(Guid originalJobId, string revisionReason)
        {
            // Validate input
            if (originalJobId == Guid.Empty)
                throw new ArgumentException("Original job ID is required", nameof(originalJobId));
                
            if (string.IsNullOrWhiteSpace(revisionReason))
                throw new ArgumentException("Revision reason is required", nameof(revisionReason));

            return await _jobRepository.CreateRevisionJobAsync(originalJobId, revisionReason);
        }
        
        // Job Parts Management
        public async Task<IEnumerable<JobPart>> GetJobPartsAsync(Guid jobId)
        {
            try
            {
                return await _jobRepository.GetJobPartsAsync(jobId);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error getting job parts in service: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to get job parts: {ex.Message}", ex);
            }
        }

        public async Task<bool> AddJobPartAsync(JobPart jobPart)
        {
            var result = await _jobRepository.AddJobPartAsync(jobPart);
            if (!result)
            {
                throw new InvalidOperationException($"Failed to add job part {jobPart.PartId} to job {jobPart.JobId}");
            }
            return result;
        }

        public async Task<bool> UpdateJobPartAsync(JobPart jobPart)
        {
            var result = await _jobRepository.UpdateJobPartAsync(jobPart);
            if (!result)
            {
                throw new InvalidOperationException($"Failed to update job part {jobPart.JobPartId}");
            }
            return result;
        }

        public async Task<bool> RemoveJobPartAsync(Guid jobPartId)
        {
            var result = await _jobRepository.RemoveJobPartAsync(jobPartId);
            if (!result)
            {
                throw new InvalidOperationException($"Failed to remove job part {jobPartId}");
            }
            return result;
        }

        public async Task<decimal> CalculateJobTotalAmountAsync(Guid jobId)
        {
            try
            {
                return await _jobRepository.CalculateJobTotalAmountAsync(jobId);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error calculating job total amount in service: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to calculate job total amount: {ex.Message}", ex);
            }
        }

        // Status Management
        public async Task<bool> UpdateJobStatusAsync(Guid jobId, JobStatus newStatus, string? changeNote = null)
        {
            return await _jobRepository.UpdateJobStatusAsync(jobId, newStatus, changeNote);
        }

        public async Task<bool> BatchUpdateStatusAsync(List<(Guid JobId, JobStatus NewStatus, string? ChangeNote)> updates)
        {
            return await _jobRepository.BatchUpdateStatusAsync(updates);
        }

        // Business Logic Validation
        public async Task<bool> CanAssignJobToTechnicianAsync(Guid jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return false;

            // Job must be in Pending status to be assigned to technician
            return job.Status == JobStatus.Pending;
        }
        
        // Workflow Validation
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

        #region Private Helper Methods

        private static bool IsValidTransition(JobStatus currentStatus, JobStatus targetStatus)
        {
            var allowedTransitions = new Dictionary<JobStatus, List<JobStatus>>
            {
                [JobStatus.Pending] = new List<JobStatus> { JobStatus.New },
                [JobStatus.New] = new List<JobStatus> { JobStatus.InProgress, JobStatus.OnHold },
                [JobStatus.InProgress] = new List<JobStatus> { JobStatus.Completed, JobStatus.OnHold },
                [JobStatus.OnHold] = new List<JobStatus> { JobStatus.InProgress, JobStatus.Completed },
                [JobStatus.Completed] = new List<JobStatus>() // Terminal status
            };

            return allowedTransitions.ContainsKey(currentStatus) &&
                   allowedTransitions[currentStatus].Contains(targetStatus);
        }

        private static List<JobStatus> GetAllowedNextStatuses(JobStatus currentStatus)
        {
            var allowedTransitions = new Dictionary<JobStatus, List<JobStatus>>
            {
                [JobStatus.Pending] = new List<JobStatus> { JobStatus.New },
                [JobStatus.New] = new List<JobStatus> { JobStatus.InProgress, JobStatus.OnHold },
                [JobStatus.InProgress] = new List<JobStatus> { JobStatus.Completed, JobStatus.OnHold },
                [JobStatus.OnHold] = new List<JobStatus> { JobStatus.InProgress, JobStatus.Completed },
                [JobStatus.Completed] = new List<JobStatus>() // Terminal status
            };

            return allowedTransitions.ContainsKey(currentStatus) ?
                   allowedTransitions[currentStatus] :
                   new List<JobStatus>();
        }

        // Create job with parts in a transaction
        public async Task<Job> CreateJobWithPartsAsync(Job job, List<JobPart> jobParts)
        {
            try
            {
                Console.WriteLine($"JobService: Creating job with {jobParts?.Count ?? 0} parts");
                var result = await _jobRepository.CreateJobWithPartsAsync(job, jobParts);
                Console.WriteLine($"JobService: Created job with ID {result.JobId}");
                return result;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error creating job with parts in service: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to create job with parts: {ex.Message}", ex);
            }
        }
        
        #endregion
        
    }
}