using BusinessObject.Quotations;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.QuotationRepositories
{
    public class QuotationItemRepository:IQuotationItemRepository
    {
        private readonly MyAppDbContext _context;

        public QuotationItemRepository(MyAppDbContext context)
        {
            _context = context;
        }

        // ✅ Lấy tất cả item của một báo giá
        public async Task<IEnumerable<QuotationItem>> GetByQuotationIdAsync(Guid quotationId)
        {
            return await _context.QuotationItems
                .Include(qi => qi.Part)
                    .ThenInclude(p => p.PartCategory)
                .Include(qi => qi.Service)
                    .ThenInclude(s => s.ServiceCategory)
                .Where(qi => qi.QuotationID == quotationId)
                .ToListAsync();
        }

        // ✅ Lấy chi tiết 1 item
        public async Task<QuotationItem?> GetByIdAsync(Guid quotationItemId)
        {
            return await _context.QuotationItems
                .Include(qi => qi.Part)
                    .ThenInclude(p => p.PartCategory)
                .Include(qi => qi.Service)
                    .ThenInclude(s => s.ServiceCategory)
                .FirstOrDefaultAsync(qi => qi.QuotationItemID == quotationItemId);
        }

        // ✅ Thêm mới item
        public async Task<QuotationItem> AddAsync(QuotationItem item)
        {
            _context.QuotationItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        // ✅ Cập nhật item
        public async Task<QuotationItem> UpdateAsync(QuotationItem item)
        {
            _context.QuotationItems.Update(item);
            await _context.SaveChangesAsync();
            return item;
        }

        // ✅ Xóa item
        public async Task<bool> DeleteAsync(Guid quotationItemId)
        {
            var item = await _context.QuotationItems.FindAsync(quotationItemId);
            if (item == null) return false;

            _context.QuotationItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
