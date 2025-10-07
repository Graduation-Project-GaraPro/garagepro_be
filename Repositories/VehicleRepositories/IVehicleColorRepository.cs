using BusinessObject.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.VehicleRepositories
{
    public interface IVehicleColorRepository
    {
        Task<IEnumerable<VehicleColor>> GetAllAsync();
        Task<VehicleColor> GetByIdAsync(Guid id);
        Task<IEnumerable<VehicleColor>> GetColorsByModelIdAsync(Guid modelId);
  
        Task AddAsync(VehicleColor color);
        Task UpdateAsync(VehicleColor color);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
