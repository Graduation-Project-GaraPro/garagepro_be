using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.FcmDataModels;
using BusinessObject.InspectionAndRepair;
using Dtos.InspectionAndRepair;
using Dtos.RoBoard;
using Microsoft.AspNetCore.SignalR;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using Repositories;
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
        private readonly IRepairOrderService _repairOrderService;
        private readonly IOrderStatusRepository _orderStatusRepository;
        private readonly Services.Notifications.INotificationService _notificationService;

        public JobTechnicianService(
            IJobTechnicianRepository jobTechnicianRepository,
            IMapper mapper,
            IHubContext<JobHub> hubContext, 
            IFcmService fcmService, 
            IUserService userService,
            IRepairOrderService repairOrderService,
            IOrderStatusRepository orderStatusRepository,
            Services.Notifications.INotificationService notificationService)
        {
            _jobTechnicianRepository = jobTechnicianRepository;
            _mapper = mapper;
            _hubContext = hubContext;
            _fcmService = fcmService;
            _userService = userService;
            _repairOrderService = repairOrderService;
            _orderStatusRepository = orderStatusRepository;
            _notificationService = notificationService;
        }

        public async Task<List<JobTechnicianDto>> GetJobsByTechnicianAsync(string userId)
        {
            var jobs = await _jobTechnicianRepository.GetJobsByTechnicianAsync(userId);

            var now = DateTimeOffset.UtcNow;
            var validStatuses = new[]
            {
        JobStatus.New,
        JobStatus.InProgress,
        JobStatus.OnHold,
        JobStatus.Completed
    };

            var filteredJobs = jobs
                .Where(j => validStatuses.Contains(j.Status) &&
                           (
                               (j.Status == JobStatus.Completed && j.Deadline.HasValue && j.Deadline.Value.Date >= now) ||
                               (j.Status == JobStatus.Completed && !j.Deadline.HasValue) ||                                                                          
                               (j.Status != JobStatus.Completed)
                           ))
                .OrderBy(j => GetStatusPriority(j.Status))
                .ThenBy(j => j.Deadline)
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
                RepairOrderId = job.RepairOrderId,          
                OldStatus = oldStatus.ToString(),
                NewStatus = dto.JobStatus.ToString(),
                UpdatedAt = DateTime.UtcNow,
                Message = $"Job status changed from {oldStatus} to {dto.JobStatus}"
            };

          

            Console.WriteLine($"[JobTechnicianService] Job {dto.JobId} status updated by technician: {oldStatus} → {dto.JobStatus}");

            // auto completed RO if all jobs are completed
            if (dto.JobStatus == JobStatus.Completed)
            {
                try
                {
                    await CheckAndCompleteRepairOrderAsync(job.RepairOrderId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[JobTechnicianService] Auto-complete RO failed: {ex.Message}");
                }
            }

            var user = await _userService.GetUserByIdAsync(job.RepairOrder.Vehicle.User.Id);


            if (user != null && user.DeviceId != null)
            {
                var FcmNotification = new FcmDataPayload
                {
                    Type = NotificationType.Repair,
                    Title = "Repair Update",
                    Body = $"Job '{job.JobName}' is now {dto.JobStatus}",
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

            // send tin hieu cho manager kh start work
            await _hubContext.Clients
                .Group("Managers")
                .SendAsync("JobStatusUpdated", new
                {
                    JobId = dto.JobId,
                    JobName = job.JobName,
                    RepairOrderId = job.RepairOrderId,
                    TechnicianId = job.JobTechnicians?.FirstOrDefault()?.TechnicianId,
                    TechnicianName = job.JobTechnicians?.FirstOrDefault()?.Technician?.User?.FullName,
                    OldStatus = oldStatus.ToString(),
                    NewStatus = dto.JobStatus.ToString(),
                    UpdatedAt = DateTime.UtcNow,
                    Message = $"Technician updated job status from {oldStatus} to {dto.JobStatus}"
                });


            return true;
        }
        public async Task<TechnicianDto?> GetTechnicianByUserIdAsync(string userId)
        {
            var technician = await _jobTechnicianRepository.GetTechnicianByUserIdAsync(userId);
            if (technician == null) return null;

            return new TechnicianDto
            {
                TechnicianId = technician.TechnicianId
            };
        }
        
        // check all jobs in RO and complete RO if all jobs are completed
        private async Task<bool> CheckAndCompleteRepairOrderAsync(Guid repairOrderId)
        {
            var allJobs = await _jobTechnicianRepository.GetJobsByRepairOrderIdAsync(repairOrderId);
            
            if (!allJobs.Any())
            {
                Console.WriteLine($"[JobTechnicianService] No jobs found for RepairOrder {repairOrderId}");
                return false;
            }

            var allCompleted = allJobs.All(j => j.Status == JobStatus.Completed);
            
            if (!allCompleted)
            {
                var incompleteCount = allJobs.Count(j => j.Status != JobStatus.Completed);
                Console.WriteLine($"[JobTechnicianService] RepairOrder {repairOrderId}: {incompleteCount} job(s) still incomplete");
                return false;
            }

            Console.WriteLine($"[JobTechnicianService] All {allJobs.Count()} jobs completed for RepairOrder {repairOrderId}. Auto-completing RO...");
            
            try
            {
                var allStatuses = await _orderStatusRepository.GetAllAsync();
                var completedStatus = allStatuses.FirstOrDefault(s => s.StatusName == "Completed");               
                
                var updateDto = new UpdateRoBoardStatusDto
                {
                    RepairOrderId = repairOrderId,
                    NewStatusId = completedStatus.OrderStatusId
                };
                
                var result = await _repairOrderService.UpdateRepairOrderStatusAsync(updateDto);
                
                if (result.Success)
                {
                    Console.WriteLine($"[JobTechnicianService] RepairOrder {repairOrderId} auto-completed successfully");
                    
                    // Send notification to managers about RO completion
                    try
                    {
                        await SendCompletionNotificationAsync(repairOrderId, true); // true = auto-completed
                    }
                    catch (Exception notificationEx)
                    {
                        Console.WriteLine($"[JobTechnicianService] Failed to send completion notification: {notificationEx.Message}");
                    }
                    
                    return true;
                }
                else
                {
                    Console.WriteLine($"[JobTechnicianService] Failed to auto-complete RepairOrder {repairOrderId}: {result.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JobTechnicianService] Exception while auto-completing RepairOrder {repairOrderId}: {ex.Message}");
                throw;
            }
        }

        private async Task SendCompletionNotificationAsync(Guid repairOrderId, bool isAutoCompleted)
        {
            try
            {
                // Get minimal repair order info for notification
                var notificationInfo = await _repairOrderService.GetNotificationInfoAsync(repairOrderId);
                if (notificationInfo == null)
                {
                    Console.WriteLine($"[JobTechnicianService] RepairOrder {repairOrderId} not found for notification");
                    return;
                }

                await _notificationService.SendRepairOrderCompletedNotificationToManagersAsync(
                    repairOrderId,
                    notificationInfo.BranchId,
                    notificationInfo.CustomerName,
                    notificationInfo.VehicleInfo,
                    isAutoCompleted
                );

                Console.WriteLine($"[JobTechnicianService] Completion notification sent for RepairOrder {repairOrderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JobTechnicianService] Error sending completion notification: {ex.Message}");
                throw;
            }
        }
    }
}