using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.QuotationRepositories
{
    public class QuotationRepository : IQuotationRepository
    {
        private readonly MyAppDbContext _context;

        public QuotationRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<Quotation> CreateAsync(Quotation quotation)
        {
            _context.Quotations.Add(quotation);
            await _context.SaveChangesAsync();
            return quotation;
        }

        public async Task<Quotation> GetByIdAsync(Guid quotationId)
        {
            return await _context.Quotations
                .Include(q => q.User)
                .Include(q => q.Vehicle)
                .Include(q => q.QuotationServices)
                .ThenInclude(qs => qs.Service)
                .Include(q => q.QuotationParts)
                .ThenInclude(qp => qp.Part)
                .FirstOrDefaultAsync(q => q.QuotationId == quotationId);
        }

        public async Task<IEnumerable<Quotation>> GetByInspectionIdAsync(Guid inspectionId)
        {
            return await _context.Quotations
                .Include(q => q.User)
                .Include(q => q.Vehicle)
                .Include(q => q.QuotationServices)
                .ThenInclude(qs => qs.Service)
                .Include(q => q.QuotationParts)
                .ThenInclude(qp => qp.Part)
                .Where(q => q.InspectionId == inspectionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quotation>> GetByUserIdAsync(string userId)
        {
            return await _context.Quotations
                .Include(q => q.User)
                .Include(q => q.Vehicle)
                .Include(q => q.QuotationServices)
                .ThenInclude(qs => qs.Service)
                .Include(q => q.QuotationParts)
                .ThenInclude(qp => qp.Part)
                .Where(q => q.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quotation>> GetAllAsync()
        {
            return await _context.Quotations
                .Include(q => q.User)
                .Include(q => q.Vehicle)
                .Include(q => q.QuotationServices)
                .ThenInclude(qs => qs.Service)
                .Include(q => q.QuotationParts)
                .ThenInclude(qp => qp.Part)
                .ToListAsync();
        }

        public async Task<Quotation> UpdateAsync(Quotation quotation)
        {
            _context.Quotations.Update(quotation);
            await _context.SaveChangesAsync();
            return quotation;
        }

        public async Task<bool> DeleteAsync(Guid quotationId)
        {
            var quotation = await _context.Quotations.FindAsync(quotationId);
            if (quotation == null)
                return false;

            _context.Quotations.Remove(quotation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid quotationId)
        {
            return await _context.Quotations.AnyAsync(q => q.QuotationId == quotationId);
        }
    }
}