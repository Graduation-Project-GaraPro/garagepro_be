using Dtos.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.VehicleServices
{
    public interface IVehicleModelService
    {
        Task<List<VehicleModelDto>> GetAllVehicleModelsAsync();
        Task<VehicleModelDto> GetVehicleModelByIdAsync(Guid modelId);
        Task<List<VehicleModelDto>> GetModelsByBrandAsync(Guid makeId);
        //Task AddVehicleModelAsync(BusinessObject.Vehicle.VehicleModel vehicleModel);
        //Task UpdateVehicleModelAsync(BusinessObject.Vehicle.VehicleModel vehicleModel);
        //Task DeleteVehicleModelAsync(Guid modelId);
    }
}
