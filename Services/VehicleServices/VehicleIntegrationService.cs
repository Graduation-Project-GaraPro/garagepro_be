using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using Dtos.RepairOrder;
using Dtos.Vehicle;
using Repositories;
using Services.VehicleServices;

namespace Services.VehicleServices
{
    public class VehicleIntegrationService : IVehicleIntegrationService
    {
        private readonly IVehicleService _vehicleService;
        private readonly IRepairOrderRepository _repairOrderRepository;
        private readonly IUserRepository _userRepository;

        public VehicleIntegrationService(
            IVehicleService vehicleService,
            IRepairOrderRepository repairOrderRepository,
            IUserRepository userRepository)
        {
            _vehicleService = vehicleService;
            _repairOrderRepository = repairOrderRepository;
            _userRepository = userRepository;
        }

        public async Task<VehicleWithHistoryDto> GetVehicleWithServiceHistoryAsync(Guid vehicleId)
        {
            // Get the vehicle details
            var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null)
                return null;

            // Get service history (repair orders) for this vehicle
            var repairOrders = await _repairOrderRepository.GetRepairOrdersWithNavigationPropertiesAsync(
                ro => ro.VehicleId == vehicleId,
                ro => ro.OrderStatus,
                ro => ro.Branch,
                ro => ro.User);

            // Get customer details
            var user = await _userRepository.GetByIdAsync(vehicle.UserId);

            var vehicleWithHistory = new VehicleWithHistoryDto
            {
                Vehicle = vehicle,
                Customer = user != null ? new CustomerDto
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = "" // Assuming address is not part of ApplicationUser
                } : null,
                ServiceHistory = repairOrders.Select(ro => new RepairOrderSummaryDto
                {
                    RepairOrderId = ro.RepairOrderId,
                    ReceiveDate = ro.ReceiveDate,
                    RepairOrderType = ro.RepairOrderType,
                    StatusName = ro.OrderStatus?.StatusName ?? "Unknown",
                    BranchName = ro.Branch?.BranchName ?? "Unknown",
                    CustomerName = ro.User?.FullName ?? "Unknown",
                    EstimatedAmount = ro.EstimatedAmount,
                    PaidAmount = ro.PaidAmount,
                    PaidStatus = ro.PaidStatus
                }).ToList()
            };

            return vehicleWithHistory;
        }

        public async Task<IEnumerable<VehicleWithCustomerDto>> GetVehiclesForCustomerAsync(string userId)
        {
            var vehicles = await _vehicleService.GetVehiclesByUserIdAsync(userId);
            var user = await _userRepository.GetByIdAsync(userId);

            var vehiclesWithCustomer = vehicles.Select(v => new VehicleWithCustomerDto
            {
                Vehicle = v,
                Customer = user != null ? new CustomerDto
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = "" // Assuming address is not part of ApplicationUser
                } : null
            }).ToList();

            return vehiclesWithCustomer;
        }

        public async Task<bool> UpdateVehicleScheduleAsync(Guid vehicleId, DateTime? nextServiceDate)
        {
            // This would be implemented in the VehicleService
            // For now, we'll just return true to indicate the operation would succeed
            return await Task.FromResult(true);
        }

        public async Task<VehicleSchedulingDto> GetVehicleSchedulingInfoAsync(Guid vehicleId)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null)
                return null;

            // Get upcoming repair orders for this vehicle
            var upcomingOrders = await _repairOrderRepository.GetRepairOrdersWithNavigationPropertiesAsync(
                ro => ro.VehicleId == vehicleId && ro.ReceiveDate > DateTime.UtcNow,
                ro => ro.OrderStatus,
                ro => ro.Branch);

            var schedulingInfo = new VehicleSchedulingDto
            {
                VehicleId = vehicle.VehicleId,
                LicensePlate = vehicle.LicensePlate,
                VIN = vehicle.VIN,
                Year = vehicle.Year,
                MakeModel = "Unknown", // This would require brand/model lookup
                NextServiceDate = vehicle.NextServiceDate,
                LastServiceDate = vehicle.LastServiceDate,
                Mileage = vehicle.Mileage,
                UpcomingAppointments = upcomingOrders.Select(ro => new RepairOrderSummaryDto
                {
                    RepairOrderId = ro.RepairOrderId,
                    ReceiveDate = ro.ReceiveDate,
                    RepairOrderType = ro.RepairOrderType,
                    StatusName = ro.OrderStatus?.StatusName ?? "Unknown",
                    BranchName = ro.Branch?.BranchName ?? "Unknown",
                    EstimatedAmount = ro.EstimatedAmount
                }).ToList()
            };

            return schedulingInfo;
        }

        public async Task<VehicleInsuranceDto> GetVehicleInsuranceInfoAsync(Guid vehicleId)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null)
                return null;

            // In a real implementation, this would connect to an insurance service
            // For now, we'll return placeholder data
            var insuranceInfo = new VehicleInsuranceDto
            {
                VehicleId = vehicle.VehicleId,
                LicensePlate = vehicle.LicensePlate,
                VIN = vehicle.VIN,
                InsuranceStatus = "Not Verified", // Placeholder
                InsuranceExpiryDate = null, // Placeholder
                InsuranceProvider = "Unknown", // Placeholder
                PolicyNumber = "Unknown" // Placeholder
            };

            return insuranceInfo;
        }
    }
}