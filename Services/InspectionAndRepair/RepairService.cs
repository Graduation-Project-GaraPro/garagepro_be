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
                throw new KeyNotFoundException("Job not found.");

            bool isAssigned = await _repairRepository.TechnicianHasJobAsync(technicianId, dto.JobId);
            if (!isAssigned)
                throw new UnauthorizedAccessException("Technician does not have permission to perform this job.");

            if (job.Status == JobStatus.Completed)
                throw new InvalidOperationException("Job is already completed, cannot create new repair.");

            if (job.Repair != null)
                throw new InvalidOperationException("Job already has a repair, cannot create another one.");

            if (string.IsNullOrWhiteSpace(dto.EstimatedTime))
                throw new ArgumentException("EstimatedTime cannot be empty.");

            if (!TryParseHoursMinutes(dto.EstimatedTime, out var estimatedTime, out var errorMessage))
                throw new ArgumentException(errorMessage);

            var repair = _mapper.Map<Repair>(dto);
            repair.EstimatedTime = estimatedTime;
            repair.StartTime = DateTime.UtcNow;

            await _repairRepository.AddRepairAsync(repair);
            job.Status = JobStatus.InProgress;
            await _repairRepository.SaveChangesAsync();

            var response = _mapper.Map<RepairResponseDto>(repair);

            // SignalR
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
                errorMessage = "Time cannot be empty.";
                return false;
            }
            var parts = input.Split(':');
            if (parts.Length != 2)
            {
                errorMessage = "Format must be 'HH:mm' (example: 25:30, 02:15, 00:45).";
                return false;
            }
            if (!int.TryParse(parts[0], out int hours))
            {
                errorMessage = "Hours must be a valid integer.";
                return false;
            }
            if (hours < 0 || hours > 200)
            {
                errorMessage = "Hours must be between 0 and 200.";
                return false;
            }
            if (!int.TryParse(parts[1], out int minutes))
            {
                errorMessage = "Minutes must be a valid integer.";
                return false;
            }

            if (minutes < 0 || minutes >= 60)
            {
                errorMessage = "Minutes must be between 0 and 59.";
                return false;
            }

            if (hours == 0 && minutes == 0)
            {
                errorMessage = "Time must be greater than 0 (00:00 is not allowed).";
                return false;
            }

            result = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);
            return true;
        }

        public async Task<Repair> UpdateRepairAsync(Guid technicianId, Guid repairId, RepairUpdateDto dto)
        {
            var repair = await _repairRepository.GetRepairByIdAsync(repairId);
            if (repair == null)
                throw new KeyNotFoundException("Repair not found.");

            var job = await _repairRepository.GetJobByIdAsync(repair.JobId);
            if (job == null)
                throw new KeyNotFoundException("Related job not found.");

            bool isAssigned = await _repairRepository.TechnicianHasJobAsync(technicianId, job.JobId);
            if (!isAssigned)
                throw new UnauthorizedAccessException("Technician does not have permission to update this job.");

            if (job.Status != JobStatus.InProgress && job.Status != JobStatus.OnHold)
                throw new InvalidOperationException("Can only update when job is InProgress or OnHold.");

            var oldDescription = repair.Description;
            var oldNotes = repair.Notes;

            _mapper.Map(dto, repair);

            await _repairRepository.UpdateRepairAsync(repair);
            await _repairRepository.SaveChangesAsync();

            // SignalR
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
                throw new KeyNotFoundException("Repair Order not found.");

            bool isAssigned = repairOrder.Jobs
                .Any(j => j.JobTechnicians.Any(t => t.TechnicianId == technicianId));

            if (!isAssigned)
                throw new UnauthorizedAccessException("You are not assigned to any job in this Repair Order.");

            var result = _mapper.Map<RepairDetailDto>(repairOrder);

            // SignalR
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