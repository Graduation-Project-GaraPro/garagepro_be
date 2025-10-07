using BusinessObject.Vehicles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Vehicles
{
    public interface IVehicleModelRepository
    {
        Task<IEnumerable<VehicleModel>> GetAllAsync();
        Task<VehicleModel> GetByIdAsync(Guid id);
        Task<IEnumerable<VehicleModel>> GetByBrandIdAsync(Guid brandId);
        Task<VehicleModel> AddAsync(VehicleModel model);
        Task<VehicleModel> UpdateAsync(VehicleModel model);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}