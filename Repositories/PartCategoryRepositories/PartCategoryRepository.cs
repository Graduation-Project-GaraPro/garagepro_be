using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories.PartCategoryRepositories
{
    public class PartCategoryRepository : IPartCategoryRepository
    {
        private readonly MyAppDbContext _context;

        public PartCategoryRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PartCategory>> GetAllWithPartsAsync()
        {
            return await _context.PartCategories
                .Include(pc => pc.Parts)
                .ToListAsync();
        }
        public IQueryable<PartCategory> Query()
        {
            return _context.PartCategories
                .Include(pc => pc.Parts)
                .AsQueryable();
        }

        public async Task<IEnumerable<PartCategory>> GetPagedAsync(int pageNumber, int pageSize, string? categoryName)
        {
            var query = _context.PartCategories
                .Include(pc => pc.Parts)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                query = query.Where(pc => pc.CategoryName.Contains(categoryName));
            }

            return await query
                .OrderBy(pc => pc.CategoryName)             // sắp xếp cho ổn định
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<PartCategory?> GetByIdWithPartsAsync(Guid id)
        {
            return await _context.PartCategories
                .Include(pc => pc.Parts)
                .FirstOrDefaultAsync(pc => pc.LaborCategoryId == id);
        }
    }

}
