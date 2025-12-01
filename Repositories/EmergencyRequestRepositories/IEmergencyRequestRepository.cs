using BusinessObject.RequestEmergency;
using Dtos.Emergency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EmergencyRequestRepositories
{
    
        public interface IEmergencyRequestRepository
        {
            Task<RequestEmergency> CreateAsync(RequestEmergency request);
            Task<IEnumerable<RequestEmergency>> GetByCustomerAsync(string customerId);
            Task<RequestEmergency?> GetByIdAsync(Guid id);
            Task<List<BranchNearbyResponseDto>> GetNearestBranchesAsync(double userLat, double userLon, int count = 5);
            Task<List<RequestEmergency>> GetAllEmergencyAsync();

                Task<bool> UpdateEmergencyStatusAsync(
                        Guid emergencyRequestId,
                        RequestEmergency.EmergencyStatus newStatus,
                        string? rejectReason = null,
                        string? technicianId = null
                    );
            Task<RequestEmergency?> GetCurrentEmergencyForTechnicianAsync(string technicianId);
            Task<List<RequestEmergency>> GetEmergencyListForTechnicianAsync(string technicianId);

            Task<bool> AssignTechnicianAsync(Guid emergencyId, string technicianId);

        Task<RequestEmergency> UpdateAsync(RequestEmergency emergency);
            Task<bool> AnyActiveAsync(string customerId, Guid vehicleId);
        Task<bool> AssignTechnicianAsync(Guid technicianUserId, Guid emergencyId);
    }
    }

