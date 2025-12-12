using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using DataAccessLayer;
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
    }
}
