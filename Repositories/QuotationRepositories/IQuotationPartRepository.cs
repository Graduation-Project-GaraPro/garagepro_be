using BusinessObject;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.QuotationRepositories
{
    public interface IQuotationPartRepository
    {
        Task<QuotationPart> CreateAsync(QuotationPart quotationPart);
        Task<QuotationPart> GetByIdAsync(Guid quotationPartId);
        Task<IEnumerable<QuotationPart>> GetByQuotationServiceIdAsync(Guid quotationServiceId);
        Task<QuotationPart> UpdateAsync(QuotationPart quotationPart);
        Task<bool> DeleteAsync(Guid quotationPartId);
        
    }
}