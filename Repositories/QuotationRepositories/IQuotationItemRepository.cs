using BusinessObject.Quotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.QuotationRepositories
{
    public interface IQuotationItemRepository
    {
        
            //  Lấy tất cả item thuộc 1 báo giá cụ thể
            Task<IEnumerable<QuotationItem>> GetByQuotationIdAsync(Guid quotationId);

            //  Lấy chi tiết item theo ID
            Task<QuotationItem?> GetByIdAsync(Guid quotationItemId);

            //  Thêm item mới (phụ tùng hoặc dịch vụ)
            Task<QuotationItem> AddAsync(QuotationItem item);

            //  Cập nhật item (số lượng, giá, v.v.)
            Task<QuotationItem> UpdateAsync(QuotationItem item);

            //  Xóa item
            Task<bool> DeleteAsync(Guid quotationItemId);
        }
    }


