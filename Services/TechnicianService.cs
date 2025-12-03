using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dtos.Job;
using Repositories;
using Repositories.InspectionAndRepair;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;

namespace Services
{
    public class TechnicianService : ITechnicianService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITechnicianRepository _technicianRepository;

        public TechnicianService(
            IJobRepository jobRepository, 
            IUserRepository userRepository,
            ITechnicianRepository technicianRepository)
        {
            _jobRepository = jobRepository;
            _userRepository = userRepository;
            _technicianRepository = technicianRepository;
        }

        public async Task<IEnumerable<TechnicianWorkloadDto>> GetAllTechnicianWorkloadsAsync(TechnicianScheduleFilterDto? filter = null)
        {
            // Get all technicians
            var technicians = await _technicianRepository.GetAllAsync();
            
            var workloadDtos = new List<TechnicianWorkloadDto>();

            foreach (var technician in technicians)
            {
                var workloadDto = await CreateTechnicianWorkloadDto(technician, filter);
                if (workloadDto != null)
                {
                    workloadDtos.Add(workloadDto);
                }
            }

            return workloadDtos;
        }

        public async Task<TechnicianWorkloadDto?> GetTechnicianWorkloadAsync(Guid technicianId)
        {
            var technician = await _technicianRepository.GetByIdAsync(technicianId);
            if (technician == null)
                return null;

            return await CreateTechnicianWorkloadDto(technician, null);
        }

        public async Task<IEnumerable<TechnicianScheduleDto>> GetAllTechnicianSchedulesAsync(TechnicianScheduleFilterDto? filter = null)
        {
            // Get all jobs with navigation properties
            var jobs = await _jobRepository.GetAllAsync();
            
            var scheduleDtos = new List<TechnicianScheduleDto>();

            foreach (var job in jobs)
            {
                // Get technicians assigned to this job
                if (job.JobTechnicians != null && job.JobTechnicians.Any())
                {
                    foreach (var jobTechnician in job.JobTechnicians)
                    {
                        var technician = jobTechnician.Technician;
                        if (technician != null)
                        {
                            var user = await _userRepository.GetByIdAsync(technician.UserId);
                            
                            var scheduleDto = CreateScheduleDto(job, technician, user);
                            scheduleDtos.Add(scheduleDto);
                        }
                    }
                }
                else
                {
                    // Job not assigned to any technician yet
                    var scheduleDto = CreateScheduleDto(job, null, null);
                    scheduleDtos.Add(scheduleDto);
                }
            }

            // Apply filters if provided
            if (filter != null)
            {
                scheduleDtos = FilterScheduleDtos(scheduleDtos, filter).ToList();
            }

            return scheduleDtos;
        }

        public async Task<IEnumerable<TechnicianScheduleDto>> GetTechnicianScheduleAsync(Guid technicianId, TechnicianScheduleFilterDto? filter = null)
        {
            // Get technician
            var technician = await _technicianRepository.GetByIdAsync(technicianId);
            if (technician == null)
                return new List<TechnicianScheduleDto>();

            var user = await _userRepository.GetByIdAsync(technician.UserId);
            
            // Get all jobs with navigation properties
            var jobs = await _jobRepository.GetAllAsync();
            
            // Filter to only jobs assigned to this technician
            var technicianJobs = jobs.Where(j => j.JobTechnicians != null && 
                j.JobTechnicians.Any(jt => jt.TechnicianId == technicianId)).ToList();

            var scheduleDtos = technicianJobs
                .Select(job => CreateScheduleDto(job, technician, user))
                .ToList();

            // Apply filters if provided
            if (filter != null)
            {
                scheduleDtos = FilterScheduleDtos(scheduleDtos, filter).ToList();
            }

            return scheduleDtos;
        }

        #region Private Helper Methods

        private async Task<TechnicianWorkloadDto?> CreateTechnicianWorkloadDto(Technician technician, TechnicianScheduleFilterDto? filter)
        {
            var user = await _userRepository.GetByIdAsync(technician.UserId);
            
            // Get all jobs for this technician
            var allJobs = await _jobRepository.GetAllAsync();
            var technicianJobs = allJobs
                .Where(j => j.JobTechnicians != null && j.JobTechnicians.Any(jt => jt.TechnicianId == technician.TechnicianId))
                .ToList();

            // Apply filters if provided
            if (filter != null)
            {
                technicianJobs = FilterJobs(technicianJobs, filter).ToList();
            }

            // Calculate performance metrics
            var performanceMetrics = CalculatePerformanceMetrics(technicianJobs);

            var workloadDto = new TechnicianWorkloadDto
            {
                TechnicianId = technician.TechnicianId,
                UserId = technician.UserId,
                FullName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown",
                TotalJobs = technicianJobs.Count,
                CompletedJobs = technicianJobs.Count(j => j.Status == JobStatus.Completed),
                InProgressJobs = technicianJobs.Count(j => j.Status == JobStatus.InProgress),
                PendingJobs = technicianJobs.Count(j => j.Status == JobStatus.Pending),
                OverdueJobs = technicianJobs.Count(j => j.Deadline.HasValue && j.Deadline.Value < DateTime.UtcNow && j.Status != JobStatus.Completed),
                AverageCompletionTime = performanceMetrics.AverageCompletionTime,
                Efficiency = performanceMetrics.Efficiency,
                Quality = performanceMetrics.Quality,
                Speed = performanceMetrics.Speed,
                Score = performanceMetrics.Score
            };

            return workloadDto;
        }

        private TechnicianScheduleDto CreateScheduleDto(Job job, Technician? technician, BusinessObject.Authentication.ApplicationUser? user)
        {
            var estimatedDuration = job.Repair?.EstimatedTime;
            var actualDuration = job.Repair?.ActualTime;

            return new TechnicianScheduleDto
            {
                JobId = job.JobId,
                JobName = job.JobName,
                TechnicianId = technician?.TechnicianId ?? Guid.Empty,
                TechnicianName = technician != null && user != null 
                    ? $"{user.FirstName} {user.LastName}".Trim() 
                    : "Unassigned",
                Status = job.Status,
                StartDate = job.Repair?.StartTime,
                Deadline = job.Deadline,
                EstimatedDuration = estimatedDuration?.TotalMinutes ?? 0,
                ActualDuration = actualDuration?.TotalMinutes ?? 0,
                IsOverdue = job.Deadline.HasValue && job.Deadline.Value < DateTime.UtcNow && job.Status != JobStatus.Completed,
                RepairOrderId = job.RepairOrderId,
                VehicleLicensePlate = job.RepairOrder?.Vehicle?.LicensePlate ?? "N/A"
            };
        }

        private PerformanceMetrics CalculatePerformanceMetrics(List<Job> jobs)
        {
            var completedJobs = jobs.Where(j => j.Status == JobStatus.Completed && j.Repair != null).ToList();
            
            if (!completedJobs.Any())
            {
                return new PerformanceMetrics
                {
                    AverageCompletionTime = 0,
                    Efficiency = 0,
                    Quality = 0,
                    Speed = 0,
                    Score = 0
                };
            }

            // Calculate average completion time (in minutes)
            var completionTimes = completedJobs
                .Where(j => j.Repair?.ActualTime != null)
                .Select(j => j.Repair!.ActualTime!.Value.TotalMinutes)
                .ToList();
            
            var averageCompletionTime = completionTimes.Any() ? (float)completionTimes.Average() : 0f;

            // Calculate Speed Score (0-100)
            // Speed = how fast they complete jobs relative to estimated time
            var speedScores = completedJobs
                .Where(j => j.Repair?.EstimatedTime != null && j.Repair?.ActualTime != null)
                .Select(j =>
                {
                    var estimated = j.Repair!.EstimatedTime!.Value.TotalMinutes;
                    var actual = j.Repair!.ActualTime!.Value.TotalMinutes;
                    
                    if (estimated == 0) return 100f;
                    
                    // If actual <= estimated, score is 100
                    // If actual > estimated, score decreases
                    var ratio = actual / estimated;
                    if (ratio <= 1.0) return 100f;
                    
                    // Penalty for going over time
                    return Math.Max(0, 100f - ((ratio - 1) * 50f));
                })
                .ToList();
            
            var speed = speedScores.Any() ? (float)speedScores.Average() : 50f;

            // Calculate Efficiency Score (0-100)
            // Efficiency = completion rate and on-time delivery
            var totalJobs = jobs.Count;
            var completed = completedJobs.Count;
            var onTimeJobs = completedJobs.Count(j => 
                !j.Deadline.HasValue || 
                (j.Repair?.EndTime != null && j.Repair.EndTime.Value <= j.Deadline.Value));
            
            var completionRate = totalJobs > 0 ? (float)completed / totalJobs : 0;
            var onTimeRate = completed > 0 ? (float)onTimeJobs / completed : 0;
            
            var efficiency = (completionRate * 50f) + (onTimeRate * 50f);

            // Calculate Quality Score (0-100)
            // Quality = low revision rate (jobs that don't need to be redone)
            var revisedJobs = jobs.Count(j => j.RevisionCount > 0);
            var qualityRate = totalJobs > 0 ? 1 - ((float)revisedJobs / totalJobs) : 1;
            var quality = qualityRate * 100f;

            // Calculate Overall Score (weighted average)
            var score = (quality * 0.4f) + (speed * 0.3f) + (efficiency * 0.3f);

            return new PerformanceMetrics
            {
                AverageCompletionTime = averageCompletionTime,
                Efficiency = efficiency,
                Quality = quality,
                Speed = speed,
                Score = score
            };
        }

        private IEnumerable<Job> FilterJobs(IEnumerable<Job> jobs, TechnicianScheduleFilterDto filter)
        {
            var filteredJobs = jobs.AsQueryable();
            
            if (filter.TechnicianId.HasValue && filter.TechnicianId.Value != Guid.Empty)
            {
                filteredJobs = filteredJobs.Where(j => j.JobTechnicians != null && 
                    j.JobTechnicians.Any(jt => jt.TechnicianId == filter.TechnicianId.Value));
            }
            
            if (filter.Status.HasValue)
            {
                filteredJobs = filteredJobs.Where(j => j.Status == filter.Status.Value);
            }
            
            if (filter.FromDate.HasValue)
            {
                filteredJobs = filteredJobs.Where(j => j.CreatedAt >= filter.FromDate.Value);
            }
            
            if (filter.ToDate.HasValue)
            {
                filteredJobs = filteredJobs.Where(j => j.CreatedAt <= filter.ToDate.Value);
            }
            
            if (filter.IsOverdueOnly.HasValue && filter.IsOverdueOnly.Value)
            {
                filteredJobs = filteredJobs.Where(j => j.Deadline.HasValue && 
                    j.Deadline.Value < DateTime.UtcNow && 
                    j.Status != JobStatus.Completed);
            }
            
            return filteredJobs;
        }

        private IEnumerable<TechnicianScheduleDto> FilterScheduleDtos(IEnumerable<TechnicianScheduleDto> scheduleDtos, TechnicianScheduleFilterDto filter)
        {
            var filteredDtos = scheduleDtos.AsQueryable();
            
            if (filter.TechnicianId.HasValue && filter.TechnicianId.Value != Guid.Empty)
            {
                filteredDtos = filteredDtos.Where(s => s.TechnicianId == filter.TechnicianId.Value);
            }
            
            if (filter.Status.HasValue)
            {
                filteredDtos = filteredDtos.Where(s => s.Status == filter.Status.Value);
            }
            
            if (filter.FromDate.HasValue)
            {
                filteredDtos = filteredDtos.Where(s => s.StartDate.HasValue && s.StartDate >= filter.FromDate.Value);
            }
            
            if (filter.ToDate.HasValue)
            {
                filteredDtos = filteredDtos.Where(s => s.StartDate.HasValue && s.StartDate <= filter.ToDate.Value);
            }
            
            if (filter.IsOverdueOnly.HasValue && filter.IsOverdueOnly.Value)
            {
                filteredDtos = filteredDtos.Where(s => s.IsOverdue);
            }
            
            return filteredDtos;
        }

        #endregion

        #region Helper Classes

        private class PerformanceMetrics
        {
            public float AverageCompletionTime { get; set; }
            public float Efficiency { get; set; }
            public float Quality { get; set; }
            public float Speed { get; set; }
            public float Score { get; set; }
        }

        #endregion
    }
}