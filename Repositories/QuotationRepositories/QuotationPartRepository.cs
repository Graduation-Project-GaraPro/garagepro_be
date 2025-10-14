using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.QuotationRepositories
{
    public class QuotationPartRepository : IQuotationPartRepository
    {
        private readonly MyAppDbContext _context;

        public QuotationPartRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<QuotationPart> CreateAsync(QuotationPart quotationPart)
        {
            _context.QuotationParts.Add(quotationPart);
            await _context.SaveChangesAsync();
            return quotationPart;
        }

        public async Task<QuotationPart> GetByIdAsync(Guid quotationPartId)
        {
            return await _context.QuotationParts
                .Include(qp => qp.Part)
                .FirstOrDefaultAsync(qp => qp.QuotationPartId == quotationPartId);
        }

        public async Task<IEnumerable<QuotationPart>> GetByQuotationIdAsync(Guid quotationId)
        {
            return await _context.QuotationParts
                .Include(qp => qp.Part)
                .Where(qp => qp.QuotationId == quotationId)
                .ToListAsync();
        }

        public async Task<QuotationPart> UpdateAsync(QuotationPart quotationPart)
        {
            _context.QuotationParts.Update(quotationPart);
            await _context.SaveChangesAsync();
            return quotationPart;
        }

        public async Task<bool> DeleteAsync(Guid quotationPartId)
        {
            var quotationPart = await _context.QuotationParts.FindAsync(quotationPartId);
            if (quotationPart == null)
                return false;

            _context.QuotationParts.Remove(quotationPart);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}