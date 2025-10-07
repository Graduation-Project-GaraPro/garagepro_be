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
         Task<List<QuotationDto>> GetQuotationsByUserIdAsync(String userId);

        //lấy chi tiết suitable cho repair request
        Task<List<QuotationDto>> GetQuotationsByRepairRequestIdAsync(String userId, Guid repairRequestId);
        //update báo giá cho phép thay đổi pârt
        //Task<QuotationDto> UpdateQuotationPartsAsync(String userId, UpdateQuotationPartsDto dto);



    }
}
