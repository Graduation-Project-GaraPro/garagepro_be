using Dtos.Vehicles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Vehicles
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
        Task<IEnumerable<VehicleDto>> GetUserVehiclesAsync(String userId);
        Task<VehicleDto> GetVehicleByIdAsync(Guid id);
        Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto vehicleDto,String UserId);
        Task<VehicleDto> UpdateVehicleAsync(Guid id, UpdateVehicleDto vehicleDto);
        Task<bool> DeleteVehicleAsync(Guid id);
    }
}