using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dtos.Vehicle;

namespace Services.VehicleServices
{
    public interface IVehicleIntegrationService
    {
        Task<VehicleWithHistoryDto> GetVehicleWithServiceHistoryAsync(Guid vehicleId);
        Task<IEnumerable<VehicleWithCustomerDto>> GetVehiclesForCustomerAsync(string userId);
        Task<bool> UpdateVehicleScheduleAsync(Guid vehicleId, DateTime? nextServiceDate);
        Task<VehicleSchedulingDto> GetVehicleSchedulingInfoAsync(Guid vehicleId);
        Task<VehicleInsuranceDto> GetVehicleInsuranceInfoAsync(Guid vehicleId);
    }
}