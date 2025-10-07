using BusinessObject.Technician;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Technician
{
    public class SpecificationRepository : ISpecificationRepository
    {
        private readonly MyAppDbContext _context;

        public SpecificationRepository(MyAppDbContext context)
        {
            _context = context;
        }


        public async Task<List<VehicleLookup>> GetAllSpecificationsAsync()
        {
            return await _context.VehicleLookups
                .Include(v => v.SpecificationsDatas)
                    .ThenInclude(sd => sd.Specification)
                        .ThenInclude(s => s.SpecificationCategory)
                .OrderBy(v => v.Automaker)
                .ThenBy(v => v.NameCar)
                .ToListAsync();
        }

        public async Task<List<VehicleLookup>> SearchSpecificationsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<VehicleLookup>();

            keyword = keyword.ToLower().Trim();

            return await _context.VehicleLookups
                .Where(v => v.Automaker.ToLower().Contains(keyword) ||
                            v.NameCar.ToLower().Contains(keyword))
                .Include(v => v.SpecificationsDatas)
                    .ThenInclude(sd => sd.Specification)
                        .ThenInclude(s => s.SpecificationCategory)
                .OrderBy(v => v.Automaker)
                .ThenBy(v => v.NameCar)
                .ToListAsync();
        }
    }
}
