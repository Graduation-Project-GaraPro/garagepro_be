using BusinessObject.Vehicles;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.VehicleRepositories
{
   
        public class VehicleColorRepository : IVehicleColorRepository
        {
            private readonly MyAppDbContext _context;

            public VehicleColorRepository(MyAppDbContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<VehicleColor>> GetAllAsync()
            {
                return await _context.VehicleColors.ToListAsync();
            }

            public async Task<VehicleColor> GetByIdAsync(Guid id)
            {
                return await _context.VehicleColors.FindAsync(id);
            }

            public async Task AddAsync(VehicleColor color)
            {
                await _context.VehicleColors.AddAsync(color);
                await _context.SaveChangesAsync();
            }

            public async Task UpdateAsync(VehicleColor color)
            {
                _context.VehicleColors.Update(color);
                await _context.SaveChangesAsync();
            }

            public async Task DeleteAsync(Guid id)
            {
                var color = await GetByIdAsync(id);
                if (color != null)
                {
                    _context.VehicleColors.Remove(color);
                    await _context.SaveChangesAsync();
                }
            }

            public async Task<bool> ExistsAsync(Guid id)
            {
                return await _context.VehicleColors.AnyAsync(c => c.ColorID == id);
            }

        public async Task<IEnumerable<VehicleColor>> GetColorsByModelIdAsync(Guid modelId)
        {
            return await _context.VehicleModelColors
                .Where(mc => mc.ModelID == modelId)
                .Select(mc => mc.Color)
                .ToListAsync();
        }



    }

}
