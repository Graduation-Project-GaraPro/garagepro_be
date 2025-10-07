using BusinessObject;
using BusinessObject.Enums; // Fix the namespace
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IQuotationRepository
    {
        Task<Quotation> GetByIdAsync(Guid id);
        Task<Quotation> GetByInspectionIdAsync(Guid inspectionId);
        Task<IEnumerable<Quotation>> GetAllAsync();
        Task<IEnumerable<Quotation>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Quotation>> GetByStatusAsync(QuotationStatus status); // Fix the type
        Task<Quotation> CreateAsync(Quotation quotation);
        Task<Quotation> UpdateAsync(Quotation quotation);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}