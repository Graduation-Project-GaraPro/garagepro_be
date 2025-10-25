using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObject;
using DataAccessLayer;
using Dtos.Quotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.QuotationRepositories
{
    public class QuotationRepository : IQuotationRepository
    {
        private readonly MyAppDbContext _context;
        private readonly IMapper _mapper;

        public QuotationRepository(MyAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<QuotationDto>> GetQuotationsByUserIdAsync(String userId)
        {
            return await _context.Inspections
                .Include(i => i.RepairOrder)
                .Include(i => i.ServiceInspections).ThenInclude(s => s.Service)
                .Include(i => i.PartInspections).ThenInclude(p => p.Part).ThenInclude(p => p.PartSpecifications)
                .Where(i => i.RepairOrder.UserId == userId)
                .ProjectTo<QuotationDto>(_mapper.ConfigurationProvider)// d�ng projectto ?? map k c?n load h?t ch? c?n nhhuwnxg entities c?n thi?t
                .ToListAsync();
        }

        public async Task<List<QuotationDto>> GetQuotationsByRepairRequestIdAsync(String userId, Guid repairRequestId)
        {
            return await _context.Inspections
                .Include(i => i.RepairOrder)
                .Include(i => i.ServiceInspections).ThenInclude(s => s.Service)
                .Include(i => i.PartInspections).ThenInclude(p => p.Part).ThenInclude(p => p.PartSpecifications)
                .Where(i => i.RepairOrder.UserId == userId && i.RepairOrderId == repairRequestId)
                .ProjectTo<QuotationDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        //update b�o gi� cho ph�p thay ??i p�rt
        //public async Task<QuotationDto> UpdateQuotationPartsAsync(String userId, UpdateQuotationPartsDto dto)
        //{
        //    var inspection = await _context.Inspections
        //        .Include(i => i.RepairOrder)
        //        .Include(i => i.PartInspections).ThenInclude(p => p.Part).ThenInclude(p => p.PartSpecifications)
        //        .FirstOrDefaultAsync(i => i.InspectionId == dto.QuotationId && i.RepairOrder.UserId == userId);

        //    if (inspection == null)
        //        throw new Exception("Quotation not found or not authorized");

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
                .FirstOrDefaultAsync(q => q.QuotationId == quotationId);
        }

        public async Task<IEnumerable<Quotation>> GetByInspectionIdAsync(Guid inspectionId)
        {
            return await _context.Quotations
                .Include(q => q.User)
                .Include(q => q.Vehicle)
                .Include(q => q.QuotationServices)
                .ThenInclude(qs => qs.Service)
                // Remove the direct QuotationParts relationship
                // .Include(q => q.QuotationParts)
                // .ThenInclude(qp => qp.Part)
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
                // Remove the direct QuotationParts relationship
                // .Include(q => q.QuotationParts)
                // .ThenInclude(qp => qp.Part)
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
                // Remove the direct QuotationParts relationship
                // .Include(q => q.QuotationParts)
                // .ThenInclude(qp => qp.Part)
                .ToListAsync();
        }
        public async Task<bool> ExistsAsync(Guid quotationId)
        {
            return await _context.Quotations.AnyAsync(q => q.QuotationId == quotationId);
        }
    }
}


