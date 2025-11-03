using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dtos.Job;
using Repositories;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;

namespace Services
{
    public class TechnicianService : ITechnicianService
    {
        private readonly IJobService _jobService;
        private readonly IUserRepository _userRepository;

        public TechnicianService(IJobService jobService, IUserRepository userRepository)
        {
            _jobService = jobService;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<TechnicianWorkloadDto>> GetAllTechnicianWorkloadsAsync(TechnicianScheduleFilterDto? filter = null)
        {
            // Get all jobs with navigation properties
            var jobs = await _jobService.GetAllJobsAsync();
            
            // Get all technicians from jobs
            var technicians = jobs
                .Where(j => j.JobTechnicians != null && j.JobTechnicians.Any())
                .SelectMany(j => j.JobTechnicians)
                .Where(jt => jt.Technician != null)
                .Select(jt => jt.Technician)
                .Distinct()
                .ToList();

            var workloadDtos = new List<TechnicianWorkloadDto>();

            foreach (var technician in technicians)
            {
                var workloadDto = await CreateTechnicianWorkloadDto(technician, jobs, filter);
                if (workloadDto != null)
                {
                    workloadDtos.Add(workloadDto);
                }
            }

            return workloadDtos;
        }

        public async Task<TechnicianWorkloadDto?> GetTechnicianWorkloadAsync(Guid technicianId)
        {
            // Get all jobs with navigation properties
            var jobs = await _jobService.GetAllJobsAsync();
            
            // Find the specific technician
            var technician = jobs
                .Where(j => j.JobTechnicians != null && j.JobTechnicians.Any())
                .SelectMany(j => j.JobTechnicians)
                .Where(jt => jt.Technician != null && jt.Technician.TechnicianId == technicianId)
                .Select(jt => jt.Technician)
                .FirstOrDefault();

            if (technician == null)
                return null;

            return await CreateTechnicianWorkloadDto(technician, jobs, null);
        }

        public async Task<IEnumerable<TechnicianScheduleDto>> GetAllTechnicianSchedulesAsync(TechnicianScheduleFilterDto? filter = null)
        {
            // Get all jobs with navigation properties
            var jobs = await _jobService.GetAllJobsAsync();
            
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
                            
                            var scheduleDto = new TechnicianScheduleDto
                            {
                                TechnicianId = technician.TechnicianId,
                                TechnicianName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown Technician",
                                JobId = job.JobId,
                                JobName = job.JobName,
                                RepairOrderId = job.RepairOrderId,
                                RepairOrderNumber = job.RepairOrderId.ToString(),
                                Status = job.Status,
                                StartTime = job.CreatedAt,
                                EndTime = job.UpdatedAt,
                                Deadline = job.Deadline,
                                EstimatedDuration = TimeSpan.FromHours(2), // This would come from the service
                                ActualDuration = job.UpdatedAt.HasValue ? job.UpdatedAt.Value - job.CreatedAt : null,
                                PriorityLevel = job.Level
                            };
                            
                            scheduleDtos.Add(scheduleDto);
                        }
                    }
                }
                else
                {
                    // Job not assigned to any technician yet
                    var scheduleDto = new TechnicianScheduleDto
                    {
                        TechnicianId = Guid.Empty,
                        TechnicianName = "Unassigned",
                        JobId = job.JobId,
                        JobName = job.JobName,
                        RepairOrderId = job.RepairOrderId,
                        RepairOrderNumber = job.RepairOrderId.ToString(),
                        Status = job.Status,
                        StartTime = job.CreatedAt,
                        EndTime = job.UpdatedAt,
                        Deadline = job.Deadline,
                        EstimatedDuration = TimeSpan.FromHours(2), // This would come from the service
                        ActualDuration = job.UpdatedAt.HasValue ? job.UpdatedAt.Value - job.CreatedAt : null,
                        PriorityLevel = job.Level
                    };
                    
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
            // Get all jobs with navigation properties
            var jobs = await _jobService.GetAllJobsAsync();
            
            // Filter to only jobs assigned to this technician
            var technicianJobs = jobs.Where(j => j.JobTechnicians != null && 
                j.JobTechnicians.Any(jt => jt.TechnicianId == technicianId)).ToList();

            var scheduleDtos = new List<TechnicianScheduleDto>();

            foreach (var job in technicianJobs)
            {
                var jobTechnician = job.JobTechnicians.FirstOrDefault(jt => jt.TechnicianId == technicianId);
                if (jobTechnician != null)
                {
                    var technician = jobTechnician.Technician;
                    if (technician != null)
                    {
                        var user = await _userRepository.GetByIdAsync(technician.UserId);
                        
                        var scheduleDto = new TechnicianScheduleDto
                        {
                            TechnicianId = technician.TechnicianId,
                            TechnicianName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown Technician",
                            JobId = job.JobId,
                            JobName = job.JobName,
                            RepairOrderId = job.RepairOrderId,
                            RepairOrderNumber = job.RepairOrderId.ToString(),
                            Status = job.Status,
                            StartTime = job.CreatedAt,
                            EndTime = job.UpdatedAt,
                            Deadline = job.Deadline,
                            EstimatedDuration = TimeSpan.FromHours(2), // This would come from the service
                            ActualDuration = job.UpdatedAt.HasValue ? job.UpdatedAt.Value - job.CreatedAt : null,
                            PriorityLevel = job.Level
                        };
                        
                        scheduleDtos.Add(scheduleDto);
                    }
                }
            }

            // Apply filters if provided
            if (filter != null)
            {
                scheduleDtos = FilterScheduleDtos(scheduleDtos, filter).ToList();
            }

            return scheduleDtos;
        }

        #region Private Helper Methods

        private async Task<TechnicianWorkloadDto?> CreateTechnicianWorkloadDto(Technician technician, IEnumerable<Job> jobs, TechnicianScheduleFilterDto? filter)
        {
            var user = await _userRepository.GetByIdAsync(technician.UserId);
            
            var technicianJobs = jobs.Where(j => j.JobTechnicians != null && 
                j.JobTechnicians.Any(jt => jt.TechnicianId == technician.TechnicianId)).ToList();

            // Apply filters if provided
            if (filter != null)
            {
                technicianJobs = FilterJobs(technicianJobs, filter).ToList();
            }

            var workloadDto = new TechnicianWorkloadDto
            {
                TechnicianId = technician.TechnicianId,
                TechnicianName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown Technician",
                TotalJobs = technicianJobs.Count,
                CompletedJobs = technicianJobs.Count(j => j.Status == JobStatus.Completed),
                InProgressJobs = technicianJobs.Count(j => j.Status == JobStatus.InProgress),
                //PendingJobs = technicianJobs.Count(j => j.Status == JobStatus.Pending || 
                //    j.Status == JobStatus.AssignedToTechnician),
            };

            // Add upcoming jobs (next 5 jobs)
            var upcomingJobs = technicianJobs
                .Where(j => j.Status != JobStatus.Completed)
                .OrderBy(j => j.Deadline ?? j.CreatedAt)
                .Take(5)
                .Select(job => new TechnicianScheduleDto
                {
                    TechnicianId = technician.TechnicianId,
                    TechnicianName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown Technician",
                    JobId = job.JobId,
                    JobName = job.JobName,
                    RepairOrderId = job.RepairOrderId,
                    RepairOrderNumber = job.RepairOrderId.ToString(),
                    Status = job.Status,
                    StartTime = job.CreatedAt,
                    EndTime = job.UpdatedAt,
                    Deadline = job.Deadline,
                    EstimatedDuration = TimeSpan.FromHours(2), // This would come from the service
                    ActualDuration = job.UpdatedAt.HasValue ? job.UpdatedAt.Value - job.CreatedAt : null,
                    PriorityLevel = job.Level
                })
                .ToList();

            workloadDto.UpcomingJobs = upcomingJobs;
            
            return workloadDto;
        }

        private IEnumerable<Job> FilterJobs(IEnumerable<Job> jobs, TechnicianScheduleFilterDto filter)
        {
            var filteredJobs = jobs.AsQueryable();
            
            // Filter by technician ID if provided
            if (filter.TechnicianId.HasValue && filter.TechnicianId.Value != Guid.Empty)
            {
                filteredJobs = filteredJobs.Where(j => j.JobTechnicians != null && 
                    j.JobTechnicians.Any(jt => jt.TechnicianId == filter.TechnicianId.Value));
            }
            
            // Filter by status if provided
            if (filter.Status.HasValue)
            {
                filteredJobs = filteredJobs.Where(j => j.Status == filter.Status.Value);
            }
            
            // Filter by date range if provided
            if (filter.FromDate.HasValue)
            {
                filteredJobs = filteredJobs.Where(j => j.CreatedAt >= filter.FromDate.Value);
            }
            
            if (filter.ToDate.HasValue)
            {
                filteredJobs = filteredJobs.Where(j => j.CreatedAt <= filter.ToDate.Value);
            }
            
            // Filter by priority level if provided
            if (filter.PriorityLevel.HasValue)
            {
                filteredJobs = filteredJobs.Where(j => j.Level == filter.PriorityLevel.Value);
            }
            
            // Filter by overdue only if requested
            if (filter.IsOverdueOnly.HasValue && filter.IsOverdueOnly.Value)
            {
                filteredJobs = filteredJobs.Where(j => j.Deadline.HasValue && j.Deadline.Value < DateTime.UtcNow && j.Status != JobStatus.Completed);
            }
            
            return filteredJobs;
        }

        private IEnumerable<TechnicianScheduleDto> FilterScheduleDtos(IEnumerable<TechnicianScheduleDto> scheduleDtos, TechnicianScheduleFilterDto filter)
        {
            var filteredDtos = scheduleDtos.AsQueryable();
            
            // Filter by technician ID if provided
            if (filter.TechnicianId.HasValue && filter.TechnicianId.Value != Guid.Empty)
            {
                filteredDtos = filteredDtos.Where(s => s.TechnicianId == filter.TechnicianId.Value);
            }
            
            // Filter by status if provided
            if (filter.Status.HasValue)
            {
                filteredDtos = filteredDtos.Where(s => s.Status == filter.Status.Value);
            }
            
            // Filter by date range if provided
            if (filter.FromDate.HasValue)
            {
                filteredDtos = filteredDtos.Where(s => s.StartTime >= filter.FromDate.Value);
            }
            
            if (filter.ToDate.HasValue)
            {
                filteredDtos = filteredDtos.Where(s => s.StartTime <= filter.ToDate.Value);
            }
            
            // Filter by priority level if provided
            if (filter.PriorityLevel.HasValue)
            {
                filteredDtos = filteredDtos.Where(s => s.PriorityLevel == filter.PriorityLevel.Value);
            }
            
            // Filter by overdue only if requested
            if (filter.IsOverdueOnly.HasValue && filter.IsOverdueOnly.Value)
            {
                filteredDtos = filteredDtos.Where(s => s.Deadline.HasValue && s.Deadline.Value < DateTime.UtcNow && s.Status != JobStatus.Completed);
            }
            
            return filteredDtos;
        }

        #endregion
    }
}