using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.RequestEmergency;

namespace Services.EmergencyRequestService
{
    public  interface ITechnicianEmergencyService
    {

        Task<TechnicianEmergencyResultDto> GetTechnicianEmergenciesAsync(string technicianId);
        Task<bool> AssignTechnicianAsync(Guid emergencyId, string technicianId);
        Task<EmergencyDetailDto> GetDetailAsync(string technicianId, Guid emergencyRequestId);
        Task<bool> UpdateEmergencyStatusAsync(
               Guid emergencyRequestId,
               RequestEmergency.EmergencyStatus newStatus,
               string? rejectReason = null,
               string? technicianId = null
           );
    }
}
