using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.FcmDataModels;
using BusinessObject.InspectionAndRepair;
using Dtos.InspectionAndRepair;
using Microsoft.AspNetCore.SignalR;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using Repositories.InspectionAndRepair;
using Services.FCMServices;
using Services.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.InspectionAndRepair
{
    public class JobTechnicianService : IJobTechnicianService
    {
        private readonly IJobTechnicianRepository _jobTechnicianRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<JobHub> _hubContext;
        private readonly IFcmService _fcmService;
        private readonly IUserService _userService;

        public JobTechnicianService(
            IJobTechnicianRepository jobTechnicianRepository,
            IMapper mapper,
            IHubContext<JobHub> hubContext, IFcmService fcmService, IUserService userService)
        {
            _jobTechnicianRepository = jobTechnicianRepository;
            _mapper = mapper;
            _hubContext = hubContext;
            _fcmService = fcmService;
            _userService = userService;
        }

        public async Task<List<JobTechnicianDto>> GetJobsByTechnicianAsync(string userId)
        {
            var jobs = await _jobTechnicianRepository.GetJobsByTechnicianAsync(userId);

            var now = DateTime.UtcNow.Date;
            var validStatuses = new[]
            {
                JobStatus.New,
                JobStatus.InProgress,
                JobStatus.OnHold,
                JobStatus.Completed
            };

            // Kết hợp filter trong một LINQ query
            var filteredJobs = jobs
                .Where(j => validStatuses.Contains(j.Status) &&
                           (j.Status != JobStatus.Completed ||
                            j.Deadline == null ||
                            j.Deadline.Value.Date >= now))
                .OrderBy(j => GetStatusPriority(j.Status))
                .ThenBy(j => j.JobName)
                .ToList();

            return _mapper.Map<List<JobTechnicianDto>>(filteredJobs);
        }

        private static int GetStatusPriority(JobStatus status)
        {
            return status switch
            {
                JobStatus.New => 1,
                JobStatus.InProgress => 2,
                JobStatus.OnHold => 3,
                JobStatus.Completed => 4,
                _ => 5
            };
        }

        public async Task<JobTechnicianDto?> GetJobByIdAsync(string userId, Guid jobId)
        {
            var jobs = await _jobTechnicianRepository.GetJobsByTechnicianAsync(userId);
            var job = jobs.FirstOrDefault(j => j.JobId == jobId);
            return job == null ? null : _mapper.Map<JobTechnicianDto>(job);
        }

        public async Task<bool> UpdateJobStatusAsync(string userId, JobStatusUpdateDto dto)
        {
            var jobs = await _jobTechnicianRepository.GetJobsByTechnicianAsync(userId);
            var job = jobs.FirstOrDefault(j => j.JobId == dto.JobId);

            if (job == null)
                throw new Exception("Công việc không tồn tại hoặc bạn không có quyền cập nhật.");

            if (job.Repair == null)
                throw new Exception("Công việc chưa có thông tin sửa chữa (Repair).");

            var validStatuses = new[] { JobStatus.InProgress, JobStatus.Completed, JobStatus.OnHold };

            if (!validStatuses.Contains(dto.JobStatus))
                throw new Exception("Chỉ được cập nhật: InProgress, Completed, OnHold.");

            if (job.Status == JobStatus.Completed && dto.JobStatus != JobStatus.Completed)
                throw new Exception("Không thể thay đổi công việc đã hoàn thành.");

            if (!validStatuses.Contains(job.Status))
                throw new Exception("Trạng thái hiện tại không hợp lệ để chuyển đổi.");

            var oldStatus = job.Status;
            DateTime? endTime = null;
            TimeSpan? actualTime = null;

            if (dto.JobStatus == JobStatus.Completed && job.Repair.StartTime.HasValue)
            {
                endTime = DateTime.UtcNow;
                actualTime = endTime.Value - job.Repair.StartTime.Value;
            }

            await _jobTechnicianRepository.UpdateJobStatusAsync(
                dto.JobId,
                dto.JobStatus,
                endTime,
                actualTime
            );

            var payload = new
            {
                JobId = dto.JobId,
                RepairOrderId = job.RepairOrderId,          // ✅ thêm RO Id vào JSON
                OldStatus = oldStatus.ToString(),
                NewStatus = dto.JobStatus.ToString(),
                UpdatedAt = DateTime.UtcNow,
                Message = $"Job status changed from {oldStatus} to {dto.JobStatus}"
            };

            var user = await _userService.GetUserByIdAsync(job.RepairOrder.Vehicle.User.Id);

           
            if (user != null && user.DeviceId != null)
            {
                var FcmNotification = new FcmDataPayload
                {
                    Type = NotificationType.Repair,
                    Title = "Repair Update",
                    Body = "Job " + job.JobName + "is  " + job.Status,
                    EntityKey = EntityKeyType.repairOrderId,
                    EntityId = job.RepairOrderId,
                    Screen = AppScreen.RepairProgressDetailFragment
                };
                await _fcmService.SendFcmMessageAsync(user?.DeviceId, FcmNotification);

                await _hubContext.Clients
               .Group($"RepairOrder_{user.Id}")
               .SendAsync("JobStatusUpdated", payload);
            }
            // Send notification
            await _hubContext.Clients
                .Group($"Job_{dto.JobId}")
                .SendAsync("JobStatusUpdated", new
                {
                    JobId = dto.JobId,
                    OldStatus = oldStatus.ToString(),
                    NewStatus = dto.JobStatus.ToString(),
                    UpdatedAt = DateTime.UtcNow,
                    Message = $"Job status changed from {oldStatus} to {dto.JobStatus}"
                });

            await _hubContext.Clients
            .Group($"RepairOrder_{job.RepairOrderId}")
            .SendAsync("JobStatusUpdated", payload);

            

            return true;
        }
    }
}