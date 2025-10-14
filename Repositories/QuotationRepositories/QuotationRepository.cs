using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObject.Quotations;
using Customers;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task<Quotation?> GetQuotationByIdAsync(Guid quotationId)
        {
            return await _context.Quotations
                .Include(q => q.RepairRequest)
                .Include(q => q.Branch)
                .Include(q => q.QuotationItems)
                .ThenInclude(qi => qi.Part)       // Nếu có Part
                .Include(q => q.QuotationItems)
                .ThenInclude(qi => qi.Service)    // Nếu có Service
                .FirstOrDefaultAsync(q => q.QuotationID == quotationId);
        }

        // Lấy tất cả báo giá mà user đó có liên quan (qua RepairRequest)
        public async Task<IEnumerable<Quotation>> GetQuotationsByUserIdAsync(string userId)
        {
            return await _context.Quotations
                .Include(q => q.RepairRequest)
                .Include(q => q.Branch)
                .Include(q => q.QuotationItems)
                .Where(q => q.RepairRequest.UserID == userId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        //  Lấy danh sách báo giá của một RepairRequest cụ thể (thuộc về user đó)
        public async Task<IEnumerable<Quotation>> GetQuotationsByRepairRequestIdAsync(string userId, Guid repairRequestId)
        {
            return await _context.Quotations
                .Include(q => q.RepairRequest)
                .Include(q => q.Branch)
                .Include(q => q.QuotationItems)
                .Where(q => q.RepairRequest.UserID == userId && q.RepairRequestID == repairRequestId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ApproveQuotationAsync(Guid quotationId)
        {
            var quotation = await _context.Quotations.FindAsync(quotationId);
            if (quotation == null) return false;

            quotation.Status = Status.Approved;
            quotation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> RejectQuotationAsync(Guid quotationId)
        {
            var quotation = await _context.Quotations.FindAsync(quotationId);
            if (quotation == null) return false;

            quotation.Status = Status.Rejected;

            quotation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Quotation> UpdateQuotationAsync(Quotation quotation)
        {
            _context.Quotations.Update(quotation);
            await _context.SaveChangesAsync();
            return quotation;
        }


    }
}



        //update báo giá cho phép thay đổi pârt
        //public async Task<QuotationDto> UpdateQuotationPartsAsync(String userId, UpdateQuotationPartsDto dto)
        //{
        //    var inspection = await _context.Inspections
        //        .Include(i => i.RepairOrder)
        //        .Include(i => i.PartInspections).ThenInclude(p => p.Part).ThenInclude(p => p.PartSpecifications)
        //        .FirstOrDefaultAsync(i => i.InspectionId == dto.QuotationId && i.RepairOrder.UserId == userId);

        //    if (inspection == null)
        //        throw new Exception("Quotation not found or not authorized");

        //    foreach (var partUpdate in dto.Parts)
        //    {
        //        var partInspection = inspection.PartInspections
        //            .FirstOrDefault(p => p.PartId == partUpdate.PartId);

        //        if (partInspection != null &&
        //            partInspection.Part.PartSpecifications.Any(s => s.SpecId == partUpdate.SelectedSpecId))
        //        {
        //        partInspection.SelectedSpecId = partUpdate.SelectedSpecId; // <-- đây mới đúng
        //    }
        //    }

        //    await _context.SaveChangesAsync();

        //    return _mapper.Map<QuotationDto>(inspection);
        //}
    



