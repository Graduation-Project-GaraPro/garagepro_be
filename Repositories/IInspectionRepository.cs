using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;

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
        Task<bool> AssignInspectionToTechnicianAsync(Guid inspectionId, Guid technicianId);
        Task<bool> UpdateInspectionFindingAsync(Guid inspectionId, string finding, string note, BusinessObject.Enums.IssueRating rating);
        Task<bool> UpdateCustomerConcernAsync(Guid inspectionId, string concern);
        Task<bool> UpdateInspectionPriceAsync(Guid inspectionId, decimal price);
        Task<bool> UpdateInspectionTypeAsync(Guid inspectionId, BusinessObject.Enums.InspectionType type);
    }
}