using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;
using Dtos.InspectionAndRepair;
using Repositories.InspectionAndRepair;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using Services.Hubs;

namespace Services.InspectionAndRepair
{
    public class RepairService : IRepairService
    {
        private readonly IRepairRepository _repairRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<RepairHub> _hubContext;

        public RepairService(
            IRepairRepository repairRepository,
            IMapper mapper,
            IHubContext<RepairHub> hubContext)
        {
            _repairRepository = repairRepository;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<RepairResponseDto> CreateRepairAsync(Guid technicianId, RepairCreateDto dto)
        {
            var job = await _repairRepository.GetJobByIdAsync(dto.JobId);
            if (job == null)
                throw new KeyNotFoundException("Không tìm thấy Job.");

            bool isAssigned = await _repairRepository.TechnicianHasJobAsync(technicianId, dto.JobId);
            if (!isAssigned)
                throw new UnauthorizedAccessException("Technician không có quyền thực hiện Job này.");

            if (job.Status == JobStatus.Completed)
                throw new InvalidOperationException("Job đã hoàn thành, không thể tạo Repair mới.");

            if (job.Repair != null)
                throw new InvalidOperationException("Job này đã có Repair, không thể tạo thêm.");

            if (string.IsNullOrWhiteSpace(dto.EstimatedTime))
                throw new ArgumentException("EstimatedTime không được để trống.");

            if (!TryParseHoursMinutes(dto.EstimatedTime, out var estimatedTime, out var errorMessage))
                throw new ArgumentException(errorMessage);

            var repair = _mapper.Map<Repair>(dto);
            repair.EstimatedTime = estimatedTime; // Set vào NotMapped property, nó sẽ tự động set EstimatedTimeTicks
            repair.StartTime = DateTime.UtcNow;

            await _repairRepository.AddRepairAsync(repair);
            job.Status = JobStatus.InProgress;
            await _repairRepository.SaveChangesAsync();

            var response = _mapper.Map<RepairResponseDto>(repair);

            // SignalR: Thông báo tạo repair thành công
            await _hubContext.Clients.Group($"RepairOrder_{job.RepairOrderId}").SendAsync("RepairCreated", new
            {
                repair.RepairId,
                repair.JobId,
                job.RepairOrderId,
                repair.Description,
                repair.Notes,
                EstimatedTime = repair.EstimatedTime.HasValue
                    ? $"{(int)repair.EstimatedTime.Value.TotalHours:D2}:{repair.EstimatedTime.Value.Minutes:D2}"
                    : null,
                repair.StartTime,
                JobStatus = job.Status.ToString()
            });

            return response;
        }

        private bool TryParseHoursMinutes(string input, out TimeSpan result, out string errorMessage)
        {
            result = TimeSpan.Zero;
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                errorMessage = "Thời gian không được để trống.";
                return false;
            }
            var parts = input.Split(':');
            if (parts.Length != 2)
            {
                errorMessage = "Format phải là 'HH:mm' (ví dụ: 25:30, 02:15, 00:45).";
                return false;
            }
            if (!int.TryParse(parts[0], out int hours))
            {
                errorMessage = "Giờ phải là số nguyên hợp lệ.";
                return false;
            }
            if (hours < 0)
            {
                errorMessage = "Giờ không được là số âm.";
                return false;
            }
            if (!int.TryParse(parts[1], out int minutes))
            {
                errorMessage = "Phút phải là số nguyên hợp lệ.";
                return false;
            }

            if (minutes < 0)
            {
                errorMessage = "Phút không được là số âm.";
                return false;
            }

            if (minutes >= 60)
            {
                errorMessage = "Phút phải từ 0 đến 59.";
                return false;
            }
            if (parts[1].Length != 2)
            {
                errorMessage = "Phút phải có đúng 2 chữ số (ví dụ: 05, 30, không phải 5 hoặc 030).";
                return false;
            }
            if (hours == 0 && minutes == 0)
            {
                errorMessage = "Thời gian phải lớn hơn 0 (không được nhập 00:00).";
                return false;
            }

            result = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);
            return true;
        }

        public async Task<Repair> UpdateRepairAsync(Guid technicianId, Guid repairId, RepairUpdateDto dto)
        {
            var repair = await _repairRepository.GetRepairByIdAsync(repairId);
            if (repair == null)
                throw new KeyNotFoundException("Không tìm thấy Repair.");

            var job = await _repairRepository.GetJobByIdAsync(repair.JobId);
            if (job == null)
                throw new KeyNotFoundException("Không tìm thấy Job liên quan.");

            bool isAssigned = await _repairRepository.TechnicianHasJobAsync(technicianId, job.JobId);
            if (!isAssigned)
                throw new UnauthorizedAccessException("Technician không có quyền cập nhật Job này.");

            if (job.Status != JobStatus.InProgress && job.Status != JobStatus.OnHold)
                throw new InvalidOperationException("Chỉ được cập nhật khi Job đang InProgress hoặc OnHold.");

            var oldDescription = repair.Description;
            var oldNotes = repair.Notes;

            _mapper.Map(dto, repair);

            await _repairRepository.UpdateRepairAsync(repair);
            await _repairRepository.SaveChangesAsync();

            // SignalR: Thông báo cập nhật repair
            await _hubContext.Clients.Group($"RepairOrder_{job.RepairOrderId}").SendAsync("RepairUpdated", new
            {
                repair.RepairId,
                repair.JobId,
                job.RepairOrderId,
                repair.Description,
                repair.Notes,
                OldDescription = oldDescription,
                OldNotes = oldNotes,
                UpdatedAt = DateTime.UtcNow
            });

            return repair;
        }

        public async Task<RepairDetailDto> GetRepairOrderDetailsAsync(Guid repairOrderId, Guid technicianId)
        {
            var repairOrder = await _repairRepository.GetRepairOrderWithJobsAsync(repairOrderId);
            if (repairOrder == null)
                throw new KeyNotFoundException("Không tìm thấy Repair Order.");

            bool isAssigned = repairOrder.Jobs
                .Any(j => j.JobTechnicians.Any(t => t.TechnicianId == technicianId));

            if (!isAssigned)
                throw new UnauthorizedAccessException("Bạn không được phân công bất kỳ Job nào trong Repair Order này.");

            var result = _mapper.Map<RepairDetailDto>(repairOrder);

            // SignalR: Thông báo đang xem (optional)
            await _hubContext.Clients.Group($"RepairOrder_{repairOrderId}").SendAsync("RepairOrderViewed", new
            {
                RepairOrderId = repairOrderId,
                TechnicianId = technicianId,
                ViewedAt = DateTime.UtcNow
            });

            return result;
        }
    }
}