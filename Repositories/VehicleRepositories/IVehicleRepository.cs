using BusinessObject;
using BusinessObject.Vehicles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;

namespace Repositories.VehicleRepositories
{
    public interface IVehicleRepository
    {
        Task<Vehicle?> GetByIdAsync(Guid vehicleId);
        Task<Vehicle?> GetByVinAsync(string vin);
        Task<Vehicle?> GetByLicensePlateAsync(string licensePlate);
        Task<IEnumerable<Vehicle>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Vehicle>> GetAllAsync();
        Task<Vehicle> CreateAsync(Vehicle vehicle);
        Task<Vehicle> UpdateAsync(Vehicle vehicle);
        IQueryable<Vehicle> Query();
        Task<bool> DeleteAsync(Guid vehicleId);
        Task<bool> ExistsAsync(Guid vehicleId);
        Task<bool> ExistsByVinAsync(string vin);
        Task<bool> ExistsByLicensePlateAsync(string licensePlate);
        Task<bool> HasRepairRequestsAsync(Guid vehicleId);
        Task<bool> HasRepairOrdersAsync(Guid vehicleId);
        Task<bool> HasQuotationsAsync(Guid vehicleId);
        Task<bool> HasEmergencyRequestsAsync(Guid vehicleId);
    }
}