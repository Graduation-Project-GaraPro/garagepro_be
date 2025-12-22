using AutoMapper;
using BusinessObject.RequestEmergency;
using Dtos.Emergency;
using Dtos.EmergencyTechnicians;
using Microsoft.AspNetCore.SignalR;
using Repositories.EmergencyRequestRepositories;
using Services.Hubs;
using Services.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.RequestEmergency.RequestEmergency;

namespace Services.EmergencyRequestService
{
    public class TechnicianEmergencyService : ITechnicianEmergencyService

    {
        private readonly IEmergencyRequestRepository _repo;
        private readonly IMapper _mapper;
        // private readonly IEmergencyRequestRepository _repository;
        private readonly IHubContext<EmergencyRequestHub> _hubContext;

        public TechnicianEmergencyService(IEmergencyRequestRepository repo, IMapper mapper, IHubContext<EmergencyRequestHub> hubContext)
        {
            _repo = repo;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<TechnicianEmergencyResultDto> GetTechnicianEmergenciesAsync(string technicianId)
        {
            var current = await _repo.GetCurrentEmergencyForTechnicianAsync(technicianId);

            var result = new TechnicianEmergencyResultDto();

            if (current != null)
            {
                result.Current = _mapper.Map<EmergencyForTechnicianDto>(current);
            }
            else
            {
                var list = await _repo.GetEmergencyListForTechnicianAsync(technicianId);
                result.List = _mapper.Map<List<EmergencyForTechnicianDto>>(list);
            }

            return result;
        }
        public async Task<EmergencyDetailDto> GetDetailAsync(string technicianId, Guid emergencyRequestId)
        {
            var entity = await _repo.GetEmergencyDetailAsync(emergencyRequestId);

            if (entity == null)
                throw new KeyNotFoundException("Not found.");

            if (!string.Equals(entity.TechnicianId, technicianId, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Unauthorized.");

            return _mapper.Map<EmergencyDetailDto>(entity);
        }



        public async Task<bool> UpdateEmergencyStatusAsync(
            Guid emergencyRequestId,
            RequestEmergency.EmergencyStatus newStatus,
            string? rejectReason = null,
            string? technicianId = null)
        {

            var result = await _repo.UpdateEmergencyStatusAsync(
                emergencyRequestId,
                newStatus,
                rejectReason,
                technicianId
            );
            if (newStatus == EmergencyStatus.InProgress)
            {
                var emergency = await _repo.GetByIdAsync(emergencyRequestId);
                if (emergency == null) return false;

                var payload = new
                {
                    EmergencyRequestId = emergency.EmergencyRequestId,
                    Status = "InProgress",
                    CustomerId = emergency.CustomerId,
                    TechnicianId = emergency.TechnicianId,
                    BranchId = emergency.BranchId,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("EmergencyRequestInProgress", payload);
                await _hubContext.Clients.Group($"customer-{emergency.CustomerId}").SendAsync("EmergencyRequestInProgress", payload);
                await _hubContext.Clients.Group($"branch-{emergency.BranchId}").SendAsync("EmergencyRequestInProgress", payload);
            }
            if (newStatus == EmergencyStatus.Towing)
            {
                var emergency = await _repo.GetByIdAsync(emergencyRequestId);
                if (emergency == null) return false;

                var payload = new
                {
                    EmergencyRequestId = emergency.EmergencyRequestId,
                    Status = "Towing",
                    CustomerId = emergency.CustomerId,
                    TechnicianId = emergency.TechnicianId,
                    BranchId = emergency.BranchId,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("EmergencyRequestTowing", payload);
                await _hubContext.Clients.Group($"customer-{emergency.CustomerId}").SendAsync("EmergencyRequestTowing", payload);
                await _hubContext.Clients.Group($"branch-{emergency.BranchId}").SendAsync("EmergencyRequestTowing", payload);
            }
            if (newStatus == EmergencyStatus.Completed)
            {
                var emergency = await _repo.GetByIdAsync(emergencyRequestId);
                if (emergency == null) return false;
                var payload = new
                {
                    EmergencyRequestId = emergency.EmergencyRequestId,
                    Status = "Completed",
                    CustomerId = emergency.CustomerId,
                    TechnicianId = emergency.TechnicianId,
                    BranchId = emergency.BranchId,
                    Timestamp = DateTime.UtcNow
                };
                await _hubContext.Clients.All.SendAsync("EmergencyRequestCompleted", payload);
                await _hubContext.Clients.Group($"customer-{emergency.CustomerId}").SendAsync("EmergencyRequestCompleted", payload);
                await _hubContext.Clients.Group($"branch-{emergency.BranchId}").SendAsync("EmergencyRequestCompleted", payload);
            }
            if (newStatus == EmergencyStatus.Assigned)
            {
                var emergency = await _repo.GetByIdAsync(emergencyRequestId);
                if (emergency == null) return false;

                var payload = new
                {
                    EmergencyRequestId = emergency.EmergencyRequestId,
                    Status = "Assigned",
                    TechnicianId = emergency.TechnicianId,
                    BranchName = emergency.Branch.BranchName,
                    TechnicianName = emergency.Technician.FirstName + emergency.Technician.LastName, // Hoặc trường tên tương ứng
                    TechnicianPhone = emergency.Technician.PhoneNumber,
                    TechnicianAvatar = emergency.Technician.AvatarUrl,
                    Message = "A technician has been assigned to your request."
                };

                // Gửi cho Khách hàng
                await _hubContext.Clients.Group($"customer-{emergency.CustomerId}").SendAsync("TechnicianAssigned", payload);
                Console.WriteLine($"RT sent: Asigned → customer-{emergency.CustomerId}, id={emergency.EmergencyRequestId}");

            }

            return result;
        }
        public async Task<bool> AssignTechnicianAsync(Guid emergencyId, string technicianId)
        {


            var assigned = await _repo.AssignTechnicianAsync(emergencyId, technicianId);
            if (!assigned) return false;

            var emergency = await _repo.GetEmergencyDetailAsync(emergencyId);
            if (emergency == null) return false;

            var payload = new
            {
                EmergencyRequestId = emergency.EmergencyRequestId,
                Status = "Assigned",
                CustomerId = emergency.CustomerId,
                TechnicianId = emergency.TechnicianId,
                BranchId = emergency.BranchId,
                Timestamp = DateTime.UtcNow,
                //TechnicianAvatar = emergency.Technician?.AvatarUrl,
                Message = "A technician has been assigned to your request."
            };
          //  await _hubContext.Clients.All.SendAsync("TechnicianAssigned", payload); ;
            await _hubContext.Clients.Group($"customer-{emergency.CustomerId}").SendAsync("TechnicianAssigned", payload);
           

            Console.WriteLine($"RT a a sent → customer-{emergency.CustomerId}, id={emergency.EmergencyRequestId}");

            return true;

        }
    }


    public class TechnicianEmergencyResultDto
    {
        public EmergencyForTechnicianDto? Current { get; set; }
        public List<EmergencyForTechnicianDto> List { get; set; } = new();
    }
    public class EmergencyDetailDto
    {
        public Guid EmergencyRequestId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }

        public Guid BranchId { get; set; }
        public string? BranchName { get; set; }
        public string? BranchAddress { get; set; }

        public Guid VehicleId { get; set; }
        public string? VehiclePlate { get; set; }
        public string? VehicleName { get; set; } 

        public string IssueDescription { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }

        public string Type { get; set; } = string.Empty;   
        public string Status { get; set; } = string.Empty; 

        public decimal? EstimatedCost { get; set; }
        public double? DistanceToGarageKm { get; set; }

        public DateTime RequestTime { get; set; }
        public DateTime? ResponseDeadline { get; set; }
        public DateTime? RespondedAt { get; set; }
        public DateTime? AutoCanceledAt { get; set; }

        public string? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }

        public string? RejectReason { get; set; }

       
    }
}
