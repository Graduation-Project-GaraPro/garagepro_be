using BusinessObject.Customers;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.RepairRequestRepositories
{
    public class RepairImageRepository:IRepairImageRepository
    {

        private readonly MyAppDbContext _context;

        public RepairImageRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RepairImage>> GetByRepairRequestIdAsync(Guid repairRequestId)
        {
            return await _context.RepairImages
                                 .Where(img => img.RepairRequestId == repairRequestId)
                                 .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid repairRequestId, string imageUrl)
        {
            return await _context.RepairImages
                                 .AnyAsync(img => img.RepairRequestId == repairRequestId
                                               && img.ImageUrl == imageUrl);
        }
        public async Task AddAsync(RepairImage image)
        {
            await _context.RepairImages.AddAsync(image);
        }

        public void Remove(RepairImage image)
        {
            _context.RepairImages.Remove(image);
        }
    }
}
