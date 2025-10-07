using BusinessObject.Vehicles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Vehicles
{
    public interface IVehicleBrandRepository
    {
        Task<IEnumerable<VehicleBrand>> GetAllAsync();
        Task<VehicleBrand> GetByIdAsync(Guid id);
        Task<VehicleBrand> AddAsync(VehicleBrand brand);
        Task<VehicleBrand> UpdateAsync(VehicleBrand brand);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}