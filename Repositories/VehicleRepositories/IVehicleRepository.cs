using BusinessObject;
using BusinessObject.Vehicles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Vehicles
{
    public interface IVehicleRepository
    {
        Task<IEnumerable<Vehicle>> GetAllAsync();
        Task<IEnumerable<Vehicle>> GetByUserIdAsync(String userId);
        Task<Vehicle> GetByIdAsync(Guid id);
        Task<Vehicle> AddAsync(Vehicle vehicle);
        Task<Vehicle> UpdateAsync(Vehicle vehicle);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}