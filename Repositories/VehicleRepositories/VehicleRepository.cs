using BusinessObject;
using BusinessObject.Vehicles;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Vehicles
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly MyAppDbContext _context;

        public VehicleRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            return await _context.Vehicles
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Color)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vehicle>> GetByUserIdAsync(String userId)
        {
            return await _context.Vehicles
                .Where(v => v.UserId == userId)
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Color)
                .ToListAsync();
        }

        public async Task<Vehicle> GetByIdAsync(Guid id)
        {
            return await _context.Vehicles
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Color)
                .FirstOrDefaultAsync(v => v.VehicleId == id);
        }

        public async Task<Vehicle> AddAsync(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<Vehicle> UpdateAsync(Vehicle vehicle)
        {
            _context.Entry(vehicle).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
                return false;

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Vehicles.AnyAsync(v => v.VehicleId == id);
        }
    }
}