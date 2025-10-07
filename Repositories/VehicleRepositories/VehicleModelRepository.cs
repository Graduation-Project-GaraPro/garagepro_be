using BusinessObject.Vehicles;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Vehicles
{
    public class VehicleModelRepository : IVehicleModelRepository
    {
        private readonly MyAppDbContext _context;

        public VehicleModelRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VehicleModel>> GetAllAsync()
        {
            return await _context.VehicleModels
                .Include(m => m.Brand)
                .ToListAsync();
        }

        public async Task<VehicleModel> GetByIdAsync(Guid id)
        {
            return await _context.VehicleModels
                .Include(m => m.Brand)
                .FirstOrDefaultAsync(m => m.ModelID == id);
        }

        public async Task<IEnumerable<VehicleModel>> GetByBrandIdAsync(Guid brandId)
        {
            return await _context.VehicleModels
                .Where(m => m.BrandID == brandId)
                .Include(m => m.Brand)
                .ToListAsync();
        }

        public async Task<VehicleModel> AddAsync(VehicleModel model)
        {
            _context.VehicleModels.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<VehicleModel> UpdateAsync(VehicleModel model)
        {
            _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var model = await _context.VehicleModels.FindAsync(id);
            if (model == null)
                return false;

            _context.VehicleModels.Remove(model);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.VehicleModels.AnyAsync(m => m.ModelID == id);
        }
    }
}