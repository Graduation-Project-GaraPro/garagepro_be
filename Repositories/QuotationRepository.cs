using BusinessObject;
using BusinessObject.Enums;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
    public class QuotationRepository : IQuotationRepository
    {
        private readonly MyAppDbContext _context;

        public QuotationRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<Quotation> GetByIdAsync(Guid id)
        {
            return await _context.Quotations
                .Include(q => q.QuotationServices)
                .ThenInclude(qs => qs.QuotationServiceParts)
                .Include(q => q.QuotationServices)
                .ThenInclude(qs => qs.Service) // Include Service
                .Include(q => q.Inspection) // Include Inspection
                .ThenInclude(i => i.RepairOrder) // Include RepairOrder
                .Include(q => q.User)
                .FirstOrDefaultAsync(q => q.QuotationId == id);
        }

        public async Task<Quotation> GetByInspectionIdAsync(Guid inspectionId)
        {
            return await _context.Quotations
                .Include(q => q.QuotationServices)
                .ThenInclude(qs => qs.QuotationServiceParts)
                .Include(q => q.QuotationServices)
                .ThenInclude(qs => qs.Service) // Include Service
                .Include(q => q.Inspection) // Include Inspection
                .ThenInclude(i => i.RepairOrder) // Include RepairOrder
                .Include(q => q.User)
                .FirstOrDefaultAsync(q => q.InspectionId == inspectionId);
        }

        public async Task<IEnumerable<Quotation>> GetAllAsync()
        {
            return await _context.Quotations
                .Include(q => q.QuotationServices)
                .ThenInclude(qs => qs.Service) // Include Service
                .Include(q => q.Inspection) // Include Inspection
                .ThenInclude(i => i.RepairOrder) // Include RepairOrder
                .Include(q => q.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quotation>> GetByUserIdAsync(string userId)
        {
            return await _context.Quotations
                .Include(q => q.QuotationServices)
                .ThenInclude(qs => qs.Service) // Include Service
                .Include(q => q.Inspection) // Include Inspection
                .ThenInclude(i => i.RepairOrder) // Include RepairOrder
                .Include(q => q.User)
                .Where(q => q.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quotation>> GetByStatusAsync(QuotationStatus status) // Fix the type
        {
            return await _context.Quotations
                .Include(q => q.QuotationServices)
                .ThenInclude(qs => qs.Service) // Include Service
                .Include(q => q.Inspection) // Include Inspection
                .ThenInclude(i => i.RepairOrder) // Include RepairOrder
                .Include(q => q.User)
                .Where(q => q.Status == status)
                .ToListAsync();
        }

        public async Task<Quotation> CreateAsync(Quotation quotation)
        {
            _context.Quotations.Add(quotation);
            await _context.SaveChangesAsync();
            return quotation;
        }

        public async Task<Quotation> UpdateAsync(Quotation quotation)
        {
            _context.Quotations.Update(quotation);
            await _context.SaveChangesAsync();
            return quotation;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var quotation = await _context.Quotations.FindAsync(id);
            if (quotation == null)
                return false;

            _context.Quotations.Remove(quotation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Quotations.AnyAsync(q => q.QuotationId == id);
        }
    }
}