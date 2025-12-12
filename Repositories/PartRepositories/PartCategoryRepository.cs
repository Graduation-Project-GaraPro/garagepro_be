using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using DataAccessLayer;
using Dtos.Parts;
using Microsoft.EntityFrameworkCore;

namespace Repositories.PartRepositories
{
    public class PartCategoryRepository : IPartCategoryRepository
    {
        private readonly MyAppDbContext _context;

        public PartCategoryRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<PartCategory> GetByIdAsync(Guid id)
        {
            return await _context.PartCategories
                .FirstOrDefaultAsync(pc => pc.LaborCategoryId == id);
        }

        public async Task<IEnumerable<PartCategory>> GetAllAsync()
        {
            return await _context.PartCategories
                .OrderBy(pc => pc.CategoryName)
                .ToListAsync();
        }

        public async Task<(IEnumerable<PartCategory> items, int totalCount)> GetPagedAsync(int page, int pageSize)
        {
            var query = _context.PartCategories.AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(pc => pc.CategoryName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(IEnumerable<PartCategory> items, int totalCount)> SearchPartCategoriesAsync(
            string searchTerm, string sortBy, string sortOrder, int page, int pageSize)
        {
            var query = _context.PartCategories.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(pc => 
                    pc.CategoryName.Contains(searchTerm) || 
                    pc.Description.Contains(searchTerm));
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "categoryname" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(pc => pc.CategoryName)
                    : query.OrderBy(pc => pc.CategoryName),
                "description" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(pc => pc.Description)
                    : query.OrderBy(pc => pc.Description),
                "createdat" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(pc => pc.CreatedAt)
                    : query.OrderBy(pc => pc.CreatedAt),
                _ => query.OrderBy(pc => pc.CategoryName)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<PartCategory> CreateAsync(PartCategory category)
        {
            category.CreatedAt = DateTime.UtcNow;
            _context.PartCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<PartCategory> UpdateAsync(PartCategory category)
        {
            category.UpdatedAt = DateTime.UtcNow;
            _context.PartCategories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await _context.PartCategories.FindAsync(id);
            if (category == null) return false;

            _context.PartCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PartCategoryWithServicesDto> GetPartCategoryWithServicesAsync(Guid id)
        {
            var category = await _context.PartCategories
                .Include(pc => pc.ServicePartCategories)
                .ThenInclude(spc => spc.Service)
                .ThenInclude(s => s.ServiceCategory)
                .Include(pc => pc.Parts)
                .FirstOrDefaultAsync(pc => pc.LaborCategoryId == id);

            if (category == null) return null;

            return new PartCategoryWithServicesDto
            {
                LaborCategoryId = category.LaborCategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                PartsCount = category.Parts?.Count ?? 0,
                Services = category.ServicePartCategories.Select(spc => new ServiceBasicDto
                {
                    ServiceId = spc.Service.ServiceId,
                    ServiceName = spc.Service.ServiceName,
                    ServiceCategoryName = spc.Service.ServiceCategory?.CategoryName ?? ""
                }).ToList()
            };
        }

        public async Task<IEnumerable<PartCategoryWithServicesDto>> GetAllPartCategoriesWithServicesAsync()
        {
            var categories = await _context.PartCategories
                .Include(pc => pc.ServicePartCategories)
                .ThenInclude(spc => spc.Service)
                .ThenInclude(s => s.ServiceCategory)
                .Include(pc => pc.Parts)
                .OrderBy(pc => pc.CategoryName)
                .ToListAsync();

            return categories.Select(category => new PartCategoryWithServicesDto
            {
                LaborCategoryId = category.LaborCategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                PartsCount = category.Parts?.Count ?? 0,
                Services = category.ServicePartCategories.Select(spc => new ServiceBasicDto
                {
                    ServiceId = spc.Service.ServiceId,
                    ServiceName = spc.Service.ServiceName,
                    ServiceCategoryName = spc.Service.ServiceCategory?.CategoryName ?? ""
                }).ToList()
            });
        }
    }
}