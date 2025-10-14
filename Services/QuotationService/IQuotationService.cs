using Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.QuotationService
{
    public interface IQuotationService
    {
        /// Lấy tất cả báo giá được gửi cho customer
         Task<IEnumerable<QuotationDto>> GetQuotationsByUserIdAsync(String userId);
        //lấy chi tiết suitable cho repair request
        Task<IEnumerable<QuotationDto>> GetQuotationsByRepairRequestIdAsync(String userId, Guid repairRequestId);
        //update báo giá cho phép thay đổi pârt
        //Task<QuotationDto> UpdateQuotationPartsAsync(String userId, UpdateQuotationPartsDto dto);
        Task<bool> ApproveQuotationAsync(Guid quotationId);
        Task<bool> RejectQuotationAsync(Guid quotationId);
    }
}
