using BusinessObject;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.QuotationRepositories
{
    public interface IQuotationServiceRepository
    {
        Task<QuotationService> CreateAsync(QuotationService quotationService);
        Task<QuotationService> GetByIdAsync(Guid quotationServiceId);
        Task<IEnumerable<QuotationService>> GetByQuotationIdAsync(Guid quotationId);
        Task<QuotationService> UpdateAsync(QuotationService quotationService);
        Task<bool> DeleteAsync(Guid quotationServiceId);
    }
}