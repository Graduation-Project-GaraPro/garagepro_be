using BusinessObject;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.QuotationRepositories
{
    public interface IQuotationRepository
    {
        Task<Quotation> CreateAsync(Quotation quotation);
        Task<Quotation> GetByIdAsync(Guid quotationId);
        Task<IEnumerable<Quotation>> GetByInspectionIdAsync(Guid inspectionId);
        Task<IEnumerable<Quotation>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Quotation>> GetAllAsync();
        Task<Quotation> UpdateAsync(Quotation quotation);
        Task<bool> DeleteAsync(Guid quotationId);
        Task<bool> ExistsAsync(Guid quotationId);
    }
}