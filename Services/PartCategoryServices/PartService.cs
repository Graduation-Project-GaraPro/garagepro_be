using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using DataAccessLayer;
using Dtos.Parts;
using Microsoft.EntityFrameworkCore;
using Repositories.PartRepositories;

namespace Services.PartCategoryServices
{
    public class PartService : IPartService
    {
        private readonly MyAppDbContext _context;
        private readonly IPartRepository _partRepository;

        public PartService(MyAppDbContext context, IPartRepository partRepository)
        {
            _context = context;
            _partRepository = partRepository;
        }

        public async Task<IEnumerable<PartDto>> GetAllPartsAsync()
        {
            var parts = await _context.Parts
                .Include(p => p.PartCategory)
                .Select(p => new PartDto
                {
                    PartId = p.PartId,
                    PartCategoryId = p.PartCategoryId,
                    BranchId = p.BranchId,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return parts;
        }

        public async Task<PartDto> GetPartByIdAsync(Guid id)
        {
            var part = await _context.Parts
                .Include(p => p.PartCategory)
                .Where(p => p.PartId == id)
                .Select(p => new PartDto
                {
                    PartId = p.PartId,
                    PartCategoryId = p.PartCategoryId,
                    BranchId = p.BranchId,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return part;
        }

        //public async Task<IEnumerable<PartByServiceDto>> GetPartsByServiceIdAsync(Guid serviceId)
        //{
        //    var parts = await _context.ServicePartCategories
        //        .Where(sp => sp.ServiceId == serviceId)
        //        .Include(sp => sp.PartCategory)
        //        .ThenInclude(p => p.Parts)
        //        .Select(sp => new PartByServiceDto
        //        {
        //            PartId = sp.Part.PartId,
        //            PartCategoryId = sp.PartCategory.LaborCategoryId,
        //            BranchId = sp.Part.BranchId,
        //            Name = sp.Part.Name,
        //            Price = sp.Part.Price,
        //            Stock = sp.Part.Stock
        //        })
        //        .ToListAsync();

        //    return parts;
        //}

        public async Task<PartDto> CreatePartAsync(CreatePartDto dto)
        {
            var part = new Part
            {
                PartCategoryId = dto.PartCategoryId,
                BranchId = dto.BranchId,
                Name = dto.Name,
                Price = dto.Price,
                Stock = dto.Stock,
                CreatedAt = DateTime.UtcNow
            };

            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            return new PartDto
            {
                PartId = part.PartId,
                PartCategoryId = part.PartCategoryId,
                BranchId = part.BranchId,
                Name = part.Name,
                Price = part.Price,
                Stock = part.Stock,
                CreatedAt = part.CreatedAt,
                UpdatedAt = part.UpdatedAt
            };
        }

        public async Task<PartDto> UpdatePartAsync(Guid id, UpdatePartDto dto)
        {
            var existingPart = await _context.Parts.FindAsync(id);
            if (existingPart == null)
                return null;

            existingPart.PartCategoryId = dto.PartCategoryId;
            existingPart.BranchId = dto.BranchId;
            existingPart.Name = dto.Name;
            existingPart.Price = dto.Price;
            existingPart.Stock = dto.Stock;
            existingPart.UpdatedAt = DateTime.UtcNow;

            _context.Parts.Update(existingPart);
            await _context.SaveChangesAsync();

            return new PartDto
            {
                PartId = existingPart.PartId,
                PartCategoryId = existingPart.PartCategoryId,
                BranchId = existingPart.BranchId,
                Name = existingPart.Name,
                Price = existingPart.Price,
                Stock = existingPart.Stock,
                CreatedAt = existingPart.CreatedAt,
                UpdatedAt = existingPart.UpdatedAt
            };
        }

        public async Task<bool> DeletePartAsync(Guid id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null)
                return false;

            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}