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
        Task<RequestEmergency> CreateEmergencyAsync(string UserId, CreateEmergencyRequestDto dto);
        Task<IEnumerable<EmergencyResponeDto>> GetByCustomerAsync(string customerId);
        Task<RequestEmergency?> GetByIdAsync(Guid id);
        Task<List<BranchNearbyResponseDto>> GetNearestBranchesAsync(double latitude,double longitude, int count = 5);
        Task<IEnumerable<RequestEmergency>> GetAllRequestEmergencyAsync();
        Task<bool> ApproveEmergency(Guid emergenciesId);
        Task<bool> RejectEmergency(Guid emergenciesId, string? reason);
    }
}
