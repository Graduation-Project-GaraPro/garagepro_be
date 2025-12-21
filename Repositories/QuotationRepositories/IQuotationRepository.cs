

using BusinessObject;
using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.QuotationRepositories
{
    public interface IQuotationRepository
    {
       
       
       
        // Task<Quotation> UpdateQuotationAsync(Quotation quotation);
        Task<Quotation> CreateAsync(Quotation quotation);
        Task<Quotation> GetByIdAsync(Guid quotationId);
        Task<IEnumerable<Quotation>> GetByInspectionIdAsync(Guid inspectionId);
        Task<IEnumerable<Quotation>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Quotation>> GetByRepairOrderIdAsync(Guid repairOrderId);
        Task<IEnumerable<Quotation>> GetAllAsync();

        Task<(List<Quotation>, int)> GetQuotationsByUserIdAsync(
        string userId,
        int pageNumber,
        int pageSize,
        QuotationStatus? status);

        Task<Quotation> UpdateAsync(Quotation quotation);
        Task<bool> DeleteAsync(Guid quotationId);
        Task<bool> ExistsAsync(Guid quotationId);

        Task<List<PartInspection>> GetPartInspectionsByInspectionIdAsync(Guid inspectionId);
        Task<PartInventory?> GetPartInventoryAsync(Guid partId, Guid branchId);
        void UpdatePartInventory(PartInventory partInventory);
    }
}