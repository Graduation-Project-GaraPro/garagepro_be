using BusinessObject;
using Dtos.Vehicles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.VehicleServices
{
    public interface IVehicleService
    {
        Task<VehicleDto?> GetVehicleByIdAsync(Guid vehicleId);
        Task<VehicleDto?> GetVehicleByVinAsync(string vin);
        Task<VehicleDto?> GetVehicleByLicensePlateAsync(string licensePlate);
        Task<IEnumerable<VehicleDto>> GetVehiclesByUserIdAsync(string userId);
        Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
        Task<IEnumerable<VehicleDto>> GetUserVehiclesAsync(string userId);
        Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto createVehicleDto);
        Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto createVehicleDto, string userId);
        Task<VehicleDto> CreateVehicleForCustomerAsync(CreateVehicleForCustomerDto createVehicleDto);
        Task<VehicleDto> UpdateVehicleAsync(Guid vehicleId, UpdateVehicleDto updateVehicleDto);
        Task<bool> DeleteVehicleAsync(Guid vehicleId);
        Task<bool> VehicleExistsAsync(Guid vehicleId);

      
            Task<List<VehicleSelectableDto>> GetUserVehiclesSelectableAsync(string userId);
        
        Task<VehicleWithCustomerDto?> GetVehicleWithCustomerAsync(Guid vehicleId);
        Task<bool> UpdateWarrantyStatusAsync(Guid vehicleId, string warrantyStatus);
        Task<bool> UpdateServiceScheduleAsync(Guid vehicleId, DateTime? nextServiceDate);
    }
}