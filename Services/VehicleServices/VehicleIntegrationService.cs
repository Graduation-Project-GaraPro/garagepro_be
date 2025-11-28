using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject;
using Dtos.RoBoard;
using Repositories;
using Repositories.VehicleRepositories;
using Dtos.Vehicles;

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
            var user = await _userRepository.GetByIdAsync(vehicle.UserID);

            var vehicleWithHistory = new VehicleWithHistoryDto
            {
                Vehicle = vehicle,
                Customer = user != null ? new RoBoardCustomerDto
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                } : null,
                ServiceHistory = repairOrders.Select(ro => new RepairOrderSummaryDto
                {
                    RepairOrderId = ro.RepairOrderId,
                    ReceiveDate = ro.ReceiveDate,
                    StatusName = ro.OrderStatus?.StatusName ?? "Unknown",
                    BranchName = ro.Branch?.BranchName ?? "Unknown",
                    CustomerName = ro.User != null ? $"{ro.User.FirstName} {ro.User.LastName}".Trim() : "Unknown",
                    EstimatedAmount = ro.EstimatedAmount,
                    PaidAmount = ro.PaidAmount,
                    PaidStatus = ro.PaidStatus.ToString()
                }).ToList()
            };

            return vehicleWithHistory;
        }

        public async Task<VehicleWithCustomerDto?> GetVehicleWithCustomerAsync(Guid vehicleId)
        {
            return await _vehicleService.GetVehicleWithCustomerAsync(vehicleId);
        }

        public async Task<IEnumerable<VehicleWithCustomerDto>> GetVehiclesForCustomerAsync(string userId)
        {
            var vehicles = await _vehicleService.GetVehiclesByUserIdAsync(userId);
            var user = await _userRepository.GetByIdAsync(userId);

            var vehiclesWithCustomer = vehicles.Select(v => new VehicleWithCustomerDto
            {
                Vehicle = v,
                Customer = user != null ? new RoBoardCustomerDto
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
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
                VehicleId = vehicle.VehicleID,
                LicensePlate = vehicle.LicensePlate,
                VIN = vehicle.VIN,
                Year = vehicle.Year,
                MakeModel = "Unknown", // This would require brand/model lookup
                NextServiceDate = vehicle.NextServiceDate,
                LastServiceDate = vehicle.LastServiceDate,
                Odometer = vehicle.Odometer,
                UpcomingAppointments = upcomingOrders.Select(ro => new RepairOrderSummaryDto
                {
                    RepairOrderId = ro.RepairOrderId,
                    ReceiveDate = ro.ReceiveDate,
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
                VehicleId = vehicle.VehicleID,
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