using BusinessObject;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.QuotationRepositories
{
    public interface IQuotationServicePartRepository
    {
        Task<QuotationServicePart> CreateAsync(QuotationServicePart quotationServicePart);
        Task<QuotationServicePart> GetByIdAsync(Guid quotationServicePartId);
        Task<IEnumerable<QuotationServicePart>> GetByQuotationServiceIdAsync(Guid quotationServiceId);
        Task<QuotationServicePart> UpdateAsync(QuotationServicePart quotationServicePart);
        Task<bool> DeleteAsync(Guid quotationServicePartId);
    }
}