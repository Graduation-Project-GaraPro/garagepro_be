using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    public class QuotationRepository: IQuotationRepository
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
                    .ProjectTo<QuotationDto>(_mapper.ConfigurationProvider)// dùng projectto để map k cần load hết chỉ cần nhhuwnxg entities cần thiết
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
    }
}


