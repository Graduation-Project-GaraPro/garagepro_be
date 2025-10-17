using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.QuotationRepositories
{
    public class QuotationServiceRepository : IQuotationServiceRepository
    {
        private readonly MyAppDbContext _context;

        public QuotationServiceRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<QuotationService> CreateAsync(QuotationService quotationService)
        {
            _context.QuotationServices.Add(quotationService);
            await _context.SaveChangesAsync();
            return quotationService;
        }

        public async Task<QuotationService> GetByIdAsync(Guid quotationServiceId)
        {
            return await _context.QuotationServices
                .Include(qs => qs.Service)
                .FirstOrDefaultAsync(qs => qs.QuotationServiceId == quotationServiceId);
        }

        public async Task<IEnumerable<QuotationService>> GetByQuotationIdAsync(Guid quotationId)
        {
            return await _context.QuotationServices
                .Include(qs => qs.Service)
                .Where(qs => qs.QuotationId == quotationId)
                .ToListAsync();
        }

        public async Task<QuotationService> UpdateAsync(QuotationService quotationService)
        {
            _context.QuotationServices.Update(quotationService);
            await _context.SaveChangesAsync();
            return quotationService;
        }

        public async Task<bool> DeleteAsync(Guid quotationServiceId)
        {
            var quotationService = await _context.QuotationServices.FindAsync(quotationServiceId);
            if (quotationService == null)
                return false;

            _context.QuotationServices.Remove(quotationService);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}