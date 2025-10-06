using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories.ServiceRepositories
{
    public class ServiceCategoryRepository : IServiceCategoryRepository
    {
        private readonly MyAppDbContext _context;

        public ServiceCategoryRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceCategory>> GetAllAsync()
        {
            return await _context.ServiceCategories
                .Include(sc => sc.Services)
                .Include(sc => sc.ChildServiceCategories)
                    .ThenInclude(c => c.Services)
                .Include(sc => sc.ChildServiceCategories)
                    .ThenInclude(c => c.ChildServiceCategories) // nếu muốn nhiều cấp
                .ToListAsync();
        }


        public async Task<ServiceCategory> GetByIdAsync(Guid id)
        {
            return await _context.ServiceCategories
                .Include(sc => sc.Services)
                .Include(sc => sc.ChildServiceCategories)
                    .ThenInclude(c => c.Services)
                .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == id);
        }


        public async Task<IEnumerable<Service>> GetServicesByCategoryIdAsync(Guid categoryId)
        {
            return await _context.Services
                .Where(s => s.ServiceCategoryId == categoryId)
                .ToListAsync();
        }
    }
}
