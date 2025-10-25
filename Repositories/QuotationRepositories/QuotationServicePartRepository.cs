using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.QuotationRepositories
{
    public class QuotationServicePartRepository : IQuotationServicePartRepository
    {
        private readonly MyAppDbContext _context;

        public QuotationServicePartRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<QuotationServicePart> CreateAsync(QuotationServicePart quotationServicePart)
        {
            _context.QuotationServiceParts.Add(quotationServicePart);
            await _context.SaveChangesAsync();
            return quotationServicePart;
        }

        public async Task<QuotationServicePart> GetByIdAsync(Guid quotationServicePartId)
        {
            return await _context.QuotationServiceParts
                .Include(qsp => qsp.Part)
                .FirstOrDefaultAsync(qsp => qsp.QuotationServicePartId == quotationServicePartId);
        }

        public async Task<IEnumerable<QuotationServicePart>> GetByQuotationServiceIdAsync(Guid quotationServiceId)
        {
            return await _context.QuotationServiceParts
                .Include(qsp => qsp.Part)
                .Where(qsp => qsp.QuotationServiceId == quotationServiceId)
                .ToListAsync();
        }

        public async Task<QuotationServicePart> UpdateAsync(QuotationServicePart quotationServicePart)
        {
            _context.QuotationServiceParts.Update(quotationServicePart);
            await _context.SaveChangesAsync();
            return quotationServicePart;
        }

        public async Task<bool> DeleteAsync(Guid quotationServicePartId)
        {
            var quotationServicePart = await _context.QuotationServiceParts.FindAsync(quotationServicePartId);
            if (quotationServicePart == null)
                return false;

            _context.QuotationServiceParts.Remove(quotationServicePart);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}