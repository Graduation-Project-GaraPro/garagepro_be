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
    public class PartRepository : IPartRepository
    {
        private readonly MyAppDbContext _context;

        public PartRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<Part> GetByIdAsync(Guid id)
        {
            return await _context.Parts
                .Include(p => p.PartCategory)
                .Include(p => p.Branch)
                .FirstOrDefaultAsync(p => p.PartId == id);
        }

        public async Task<IEnumerable<Part>> GetAllAsync()
        {
            return await _context.Parts
                .Include(p => p.PartCategory)
                .Include(p => p.Branch)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Part>> GetByBranchIdAsync(Guid branchId)
        {
            return await _context.Parts
                .Include(p => p.PartCategory)
                .Include(p => p.Branch)
                .Where(p => p.BranchId == branchId)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Part> items, int totalCount)> GetPagedAsync(int page, int pageSize)
        {
            var query = _context.Parts
                .Include(p => p.PartCategory)
                .Include(p => p.Branch)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(IEnumerable<Part> items, int totalCount)> GetPagedByBranchAsync(Guid branchId, int page, int pageSize)
        {
            var query = _context.Parts
                .Include(p => p.PartCategory)
                .Include(p => p.Branch)
                .Where(p => p.BranchId == branchId)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Part> CreateAsync(Part part)
        {
            part.CreatedAt = DateTime.UtcNow;
            _context.Parts.Add(part);
            await _context.SaveChangesAsync();
            
            // Reload with includes
            return await GetByIdAsync(part.PartId);
        }

        public async Task<Part> UpdateAsync(Part part)
        {
            part.UpdatedAt = DateTime.UtcNow;
            _context.Parts.Update(part);
            await _context.SaveChangesAsync();
            
            // Reload with includes
            return await GetByIdAsync(part.PartId);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null) return false;

            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<Part> Items, int TotalCount)> SearchPartsAsync(
            string searchTerm,
            Guid? partCategoryId,
            Guid? branchId,
            decimal? minPrice,
            decimal? maxPrice,
            string sortBy,
            string sortOrder,
            int page,
            int pageSize)
        {
            var query = _context.Parts
                .Include(p => p.PartCategory)
                .Include(p => p.Branch)
                .AsQueryable();

            // Filter by search term (name)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm));
            }

            // Filter by category
            if (partCategoryId.HasValue)
            {
                query = query.Where(p => p.PartCategoryId == partCategoryId.Value);
            }

            // Filter by branch
            if (branchId.HasValue)
            {
                query = query.Where(p => p.BranchId == branchId.Value);
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Sorting
            query = sortBy?.ToLower() switch
            {
                "price" => sortOrder?.ToLower() == "desc" 
                    ? query.OrderByDescending(p => p.Price) 
                    : query.OrderBy(p => p.Price),
                "stock" => sortOrder?.ToLower() == "desc" 
                    ? query.OrderByDescending(p => p.Stock) 
                    : query.OrderBy(p => p.Stock),
                "createdat" => sortOrder?.ToLower() == "desc" 
                    ? query.OrderByDescending(p => p.CreatedAt) 
                    : query.OrderBy(p => p.CreatedAt),
                _ => sortOrder?.ToLower() == "desc" 
                    ? query.OrderByDescending(p => p.Name) 
                    : query.OrderBy(p => p.Name)
            };

            // Pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<Part>> GetPartsForServiceAsync(Guid serviceId)
        {
            return await _context.Parts
                .Include(p => p.PartCategory)
                .Include(p => p.Branch)
                .Where(p => p.PartCategory.ServicePartCategories.Any(spc => spc.ServiceId == serviceId))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> UpdateServicePartCategoriesAsync(Guid serviceId, List<Guid> partCategoryIds)
        {
            // Remove existing relationships
            var existingRelationships = await _context.ServicePartCategories
                .Where(spc => spc.ServiceId == serviceId)
                .ToListAsync();

            _context.ServicePartCategories.RemoveRange(existingRelationships);

            // Add new relationships
            var newRelationships = partCategoryIds.Select(categoryId => new ServicePartCategory
            {
                ServiceId = serviceId,
                PartCategoryId = categoryId
            });

            _context.ServicePartCategories.AddRange(newRelationships);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ServicePartCategoryDto> GetServiceWithPartCategoriesAsync(Guid serviceId)
        {
            var service = await _context.Services
                .Include(s => s.ServicePartCategories)
                .ThenInclude(spc => spc.PartCategory)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

            if (service == null) return null;

            return new ServicePartCategoryDto
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                PartCategories = service.ServicePartCategories.Select(spc => new PartCategoryDto
                {
                    LaborCategoryId = spc.PartCategory.LaborCategoryId,
                    CategoryName = spc.PartCategory.CategoryName,
                    Description = spc.PartCategory.Description,
                    CreatedAt = spc.PartCategory.CreatedAt,
                    UpdatedAt = spc.PartCategory.UpdatedAt
                }).ToList()
            };
        }
    }
}
