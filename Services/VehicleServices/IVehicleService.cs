using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;
using Dtos.Vehicle;

namespace Services.VehicleServices
{
    public interface IVehicleService
    {
        Task<VehicleDto?> GetVehicleByIdAsync(Guid vehicleId);
        Task<VehicleDto?> GetVehicleByVinAsync(string vin);
        Task<VehicleDto?> GetVehicleByLicensePlateAsync(string licensePlate);
        Task<IEnumerable<VehicleDto>> GetVehiclesByUserIdAsync(string userId);
        Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
        Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto createVehicleDto);
        Task<VehicleDto> UpdateVehicleAsync(Guid vehicleId, UpdateVehicleDto updateVehicleDto);
        Task<bool> DeleteVehicleAsync(Guid vehicleId);
        Task<bool> VehicleExistsAsync(Guid vehicleId);
        Task<VehicleWithCustomerDto?> GetVehicleWithCustomerAsync(Guid vehicleId);
        Task<bool> UpdateWarrantyStatusAsync(Guid vehicleId, string warrantyStatus);
        Task<bool> UpdateServiceScheduleAsync(Guid vehicleId, DateTime? nextServiceDate);
    }
}