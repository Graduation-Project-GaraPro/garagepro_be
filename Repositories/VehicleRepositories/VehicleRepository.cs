using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories.VehicleRepositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly MyAppDbContext _context;

        public VehicleRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<Vehicle?> GetByIdAsync(Guid vehicleId)
        {
            return await _context.Vehicles
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);
        }

        public async Task<Vehicle?> GetByVinAsync(string vin)
        {
            return await _context.Vehicles
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.VIN == vin);
        }

        public async Task<Vehicle?> GetByLicensePlateAsync(string licensePlate)
        {
            return await _context.Vehicles
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);
        }

        public async Task<IEnumerable<Vehicle>> GetByUserIdAsync(string userId)
        {
            return await _context.Vehicles
                .Include(v => v.User)
                .Where(v => v.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            return await _context.Vehicles
                .Include(v => v.User)
                .ToListAsync();
        }

        public async Task<Vehicle> CreateAsync(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<Vehicle> UpdateAsync(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<bool> DeleteAsync(Guid vehicleId)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null) return false;

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid vehicleId)
        {
            return await _context.Vehicles.AnyAsync(v => v.VehicleId == vehicleId);
        }

        public async Task<bool> ExistsByVinAsync(string vin)
        {
            return await _context.Vehicles.AnyAsync(v => v.VIN == vin);
        }

        public async Task<bool> ExistsByLicensePlateAsync(string licensePlate)
        {
            return await _context.Vehicles.AnyAsync(v => v.LicensePlate == licensePlate);
        }
    }
}