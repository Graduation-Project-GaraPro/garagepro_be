using BusinessObject.RequestEmergency;
using Dtos.Emergency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.EmergencyRequestService
{
    public interface IEmergencyRequestService
    {
        Task<EmergencyResponeDto> CreateEmergencyAsync(string UserId, CreateEmergencyRequestDto dto, string? idempotencyKey = null);
        Task<IEnumerable<EmergencyResponeDto>> GetByCustomerAsync(string customerId);
        Task<RequestEmergency?> GetByIdAsync(Guid id);
        Task<EmergencyResponeDto?> GetDtoByIdAsync(Guid id);
        Task<string> ReverseGeocodeAddressAsync(double latitude, double longitude);
        Task<List<BranchNearbyResponseDto>> GetNearestBranchesAsync(double latitude,double longitude, int count = 5);
        Task<IEnumerable<RequestEmergency>> GetAllRequestEmergencyAsync();
        Task<bool> ApproveEmergency(Guid emergenciesId, string managerUserId);
        Task<bool> RejectEmergency(Guid emergenciesId, string? reason);
        Task<bool> SetInProgressAsync(Guid emergenciesId);
        Task<bool> CancelEmergencyAsync(string userId, Guid emergenciesId);
        Task<RouteDto> GetRouteByEmergencyIdAsync(Guid routeId);
        Task<RouteDto> GetRouteAsync(double startLat, double startLng, double endLat, double endLng);
        Task<bool> UpdateTechnicianLocationAsync(string technicianUserId, TechnicianLocationDto location);
        Task<bool> AsignTechnicianToEmergencyAsync(string emergencyId, Guid technicianUserId);
    }
}
