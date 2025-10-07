using BusinessObject.Vehicles;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Vehicles
{
    public class VehicleBrandRepository : IVehicleBrandRepository
    {
        private readonly MyAppDbContext _context;

        public VehicleBrandRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VehicleBrand>> GetAllAsync()
        {
            return await _context.VehicleBrands
                .Include(b => b.VehicleModels)
                .ToListAsync();
        }

        public async Task<VehicleBrand> GetByIdAsync(Guid id)
        {
            return await _context.VehicleBrands
                .Include(b => b.VehicleModels)
                .FirstOrDefaultAsync(b => b.BrandID == id);
        }

        public async Task<VehicleBrand> AddAsync(VehicleBrand brand)
        {
            _context.VehicleBrands.Add(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task<VehicleBrand> UpdateAsync(VehicleBrand brand)
        {
            _context.Entry(brand).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var brand = await _context.VehicleBrands.FindAsync(id);
            if (brand == null)
                return false;

            _context.VehicleBrands.Remove(brand);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.VehicleBrands.AnyAsync(b => b.BrandID == id);
        }
    }
}