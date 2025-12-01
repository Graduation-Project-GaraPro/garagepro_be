using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.RequestEmergency;
using Dtos.EmergencyTechnicians;
using Microsoft.AspNetCore.SignalR;
using Repositories.EmergencyRequestRepositories;
using Services.Hubs;

namespace Services.EmergencyRequestService
{
    public class TechnicianEmergencyService :  ITechnicianEmergencyService

    {
        private readonly IEmergencyRequestRepository _repo;
        private readonly IMapper _mapper;
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

            await _hubContext.Clients.Group($"emergency-{emergencyRequestId}")
                    .SendAsync("EmergencyStatusUpdated", newStatus);

            return result;
        }
        public async Task<bool> AssignTechnicianAsync(Guid emergencyId, string technicianId)
        {
            return await _repo.AssignTechnicianAsync(emergencyId, technicianId);
        }
    }

    public class TechnicianEmergencyResultDto
    {
        public EmergencyForTechnicianDto? Current { get; set; }
        public List<EmergencyForTechnicianDto> List { get; set; } = new();
    }
}
