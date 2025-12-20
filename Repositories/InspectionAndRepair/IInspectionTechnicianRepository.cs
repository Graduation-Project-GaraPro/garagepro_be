using BusinessObject;
using Microsoft.EntityFrameworkCore;
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
        Task<List<Service>> GetAllServicesAsync();
        void AddServiceInspection(ServiceInspection serviceInspection);
        void AddPartInspection(PartInspection partInspection);
        void RemovePartInspections(IEnumerable<PartInspection> partInspections);
        Task<bool> HasRepairOrderServicesAsync(Guid repairOrderId);
        Task<Service?> GetServiceByIdAsync(Guid serviceId);
        void RemoveServiceInspection(ServiceInspection serviceInspection);
        Task SaveChangesAsync();
        Task<PartInventory?> GetPartInventoryAsync(Guid partId, Guid branchId);
        void UpdatePartInventory(PartInventory partInventory);
        Task<Inspection?> GetInspectionWithRepairOrderAsync(Guid inspectionId);
    }
}
