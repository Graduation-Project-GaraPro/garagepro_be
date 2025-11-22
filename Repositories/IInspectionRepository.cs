using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.InspectionAndRepair;

namespace Repositories
{
    public interface IInspectionRepository
    {
        // Basic CRUD operations
        Task<Inspection> GetByIdAsync(Guid inspectionId);
        Task<IEnumerable<Inspection>> GetAllAsync();
        Task<IEnumerable<Inspection>> GetByRepairOrderIdAsync(Guid repairOrderId);
        Task<IEnumerable<Inspection>> GetByTechnicianIdAsync(Guid technicianId);
        Task<Inspection> CreateAsync(Inspection inspection);
        Task<Inspection> UpdateAsync(Inspection inspection);
        Task<bool> DeleteAsync(Guid inspectionId);
        Task<bool> ExistsAsync(Guid inspectionId);


        // Additional operations
        Task<IEnumerable<Inspection>> GetPendingInspectionsAsync();
        Task<IEnumerable<Inspection>> GetCompletedInspectionsAsync();
        Task<IEnumerable<Inspection>> GetCompletedInspectionsWithDetailsAsync();
        Task<bool> AssignInspectionToTechnicianAsync(Guid inspectionId, Guid technicianId);
        Task<Inspection?> GetInspectionByIdAsync(Guid inspectionId);
        Task<Technician?> GetTechnicianByIdAsync(Guid technicianId);
        Task<string> GetUserIdByTechnicianIdAsync(Guid technicianId);
    }
}