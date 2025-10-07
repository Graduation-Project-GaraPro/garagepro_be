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

        public async Task<PartCategory?> GetByIdWithPartsAsync(Guid id)
        {
            return await _context.PartCategories
                .Include(pc => pc.Parts)
                .FirstOrDefaultAsync(pc => pc.LaborCategoryId == id);
        }
    }

}
