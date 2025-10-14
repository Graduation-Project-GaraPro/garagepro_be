using BusinessObject.Quotations;
using Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.QuotationRepositories
{
    public interface IQuotationRepository
    {
       
        /// Lấy tất cả báo giá được gửi cho customer
         Task<IEnumerable<Quotation>> GetQuotationsByUserIdAsync(String userId);

        //lấy chi tiết suitable cho repair request
        Task<IEnumerable<Quotation>> GetQuotationsByRepairRequestIdAsync(String userId, Guid repairRequestId);
        //update báo giá cho phép thay đổi pârt
        Task<Quotation> GetQuotationByIdAsync(Guid quotationId);
        //Task<QuotationDto> UpdateQuotationPartsAsync(String userId, UpdateQuotationPartsDto dto);
        Task<bool> ApproveQuotationAsync(Guid quotationId);
        Task<bool> RejectQuotationAsync(Guid quotationId);
         Task<Quotation> UpdateQuotationAsync(Quotation quotation);




    }
}
