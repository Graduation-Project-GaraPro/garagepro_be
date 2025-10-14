using BusinessObject;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.InspectionAndRepair
{
    public interface IInspectionTechnicianRepository
    {
        
        Task<BusinessObject.InspectionAndRepair.Technician?> GetTechnicianByUserIdAsync(string userId);

        
        Task<List<Inspection>> GetInspectionsByTechnicianIdAsync(Guid technicianId);
        Task<Inspection?> GetInspectionByIdAndTechnicianIdAsync(Guid inspectionId, Guid technicianId);
       
        Task<List<RepairOrderService>> GetRepairOrderServicesAsync(Guid repairOrderId);

        void AddServiceInspection(ServiceInspection serviceInspection);
        void AddPartInspection(PartInspection partInspection);
        void RemovePartInspections(IEnumerable<PartInspection> partInspections);
        Task SaveChangesAsync();
    }
}
