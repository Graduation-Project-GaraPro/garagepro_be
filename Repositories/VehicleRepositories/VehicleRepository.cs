using BusinessObject;
using BusinessObject.Vehicles;
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
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Color)
                .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);
        }

        public async Task<Vehicle?> GetByVinAsync(string vin)
        {
            return await _context.Vehicles
                .Include(v => v.User)
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Color)
                .FirstOrDefaultAsync(v => v.VIN == vin);
        }

        public async Task<Vehicle?> GetByLicensePlateAsync(string licensePlate)
        {
            return await _context.Vehicles
                .Include(v => v.User)
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Color)
                .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);
        }

        public async Task<IEnumerable<Vehicle>> GetByUserIdAsync(string userId)
        {
            return await _context.Vehicles
                .Include(v => v.User)
                .Where(v => v.UserId == userId)
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Color)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            return await _context.Vehicles
                .Include(v => v.User)
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Color)
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

        public async Task<bool> HasRepairRequestsAsync(Guid vehicleId)
        {
            return await _context.RepairRequests.AnyAsync(r => r.VehicleID == vehicleId);
        }

        public async Task<bool> HasRepairOrdersAsync(Guid vehicleId)
        {
            return await _context.RepairOrders.AnyAsync(ro => ro.VehicleId == vehicleId);
        }

        public async Task<bool> HasQuotationsAsync(Guid vehicleId)
        {
            return await _context.Quotations.AnyAsync(q => q.VehicleId == vehicleId);
        }

        public async Task<bool> HasEmergencyRequestsAsync(Guid vehicleId)
        {
            return await _context.RequestEmergencies.AnyAsync(e => e.VehicleId == vehicleId);
        }
    }
}