using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject;
using Dtos.RoBoard;
using Repositories.VehicleRepositories;
using Dtos.Vehicles;

namespace Services.VehicleServices
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IMapper _mapper;

        public VehicleService(IVehicleRepository vehicleRepository, IMapper mapper)
        {
            _vehicleRepository = vehicleRepository;
            _mapper = mapper;
        }

        public async Task<VehicleDto?> GetVehicleByIdAsync(Guid vehicleId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            return vehicle == null ? null : _mapper.Map<VehicleDto>(vehicle);
        }

        public async Task<VehicleDto?> GetVehicleByVinAsync(string vin)
        {
            var vehicle = await _vehicleRepository.GetByVinAsync(vin);
            return vehicle == null ? null : _mapper.Map<VehicleDto>(vehicle);
        }

        public async Task<VehicleDto?> GetVehicleByLicensePlateAsync(string licensePlate)
        {
            var vehicle = await _vehicleRepository.GetByLicensePlateAsync(licensePlate);
            return vehicle == null ? null : _mapper.Map<VehicleDto>(vehicle);
        }

        public async Task<IEnumerable<VehicleDto>> GetVehiclesByUserIdAsync(string userId)
        {
            var vehicles = await _vehicleRepository.GetByUserIdAsync(userId);
            return vehicles.Select(v => _mapper.Map<VehicleDto>(v));
        }

        public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync()
        {
            var vehicles = await _vehicleRepository.GetAllAsync();
            return vehicles.Select(v => _mapper.Map<VehicleDto>(v));
        }

        public async Task<IEnumerable<VehicleDto>> GetUserVehiclesAsync(string userId)
        {
            var vehicles = await _vehicleRepository.GetByUserIdAsync(userId);
            return vehicles.Select(v => _mapper.Map<VehicleDto>(v));
        }

        // This method is used when the user ID is provided in the DTO
        public async Task<VehicleDto> CreateVehicleAsync( CreateVehicleDto createVehicleDto)
        {
            var vehiclesExits = await _vehicleRepository.GetByUserIdAsync(createVehicleDto.UserID);
            var vin = createVehicleDto.VIN;
            if (!string.IsNullOrWhiteSpace(vin) && vehiclesExits.Any(v => v.VIN == vin))
            {
                throw new Exception("VIN already exists for this user");
            }

            if (vehiclesExits.Any(v => v.LicensePlate == createVehicleDto.LicensePlate))
            {
                throw new Exception("License plate already exists for this user");
            }

            var vehicle = new Vehicle
            {
                BrandId = createVehicleDto.BrandID,
                UserId = createVehicleDto.UserID,
                ModelId = createVehicleDto.ModelID,
                ColorId = createVehicleDto.ColorID,
                LicensePlate = createVehicleDto.LicensePlate,
                VIN = createVehicleDto.VIN,
                Year = createVehicleDto.Year,
                Odometer = createVehicleDto.Odometer,
                LastServiceDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            
            var createdVehicle = await _vehicleRepository.CreateAsync(vehicle);
            return _mapper.Map<VehicleDto>(createdVehicle);
        }

        // This method is used when the user ID should be taken from the authenticated user
        public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto createVehicleDto, string userId)
        {
            // Override the user ID with the authenticated user's ID
            var vehiclesExits = await _vehicleRepository.GetByUserIdAsync(userId);

            var vin = createVehicleDto.VIN;
            if (!string.IsNullOrWhiteSpace(vin) && vehiclesExits.Any(v => v.VIN == vin))
            {
                throw new Exception("VIN already exists for this user");
            }

            if (vehiclesExits.Any(v => v.LicensePlate == createVehicleDto.LicensePlate))
            {
                throw new Exception("License plate already exists for this user");
            }

            createVehicleDto.UserID = userId;
            
            // Create a new Vehicle entity and map the properties
            var vehicle = new Vehicle
            {
                BrandId = createVehicleDto.BrandID,
                UserId = createVehicleDto.UserID, // Use the authenticated user's ID
                ModelId = createVehicleDto.ModelID,
                ColorId = createVehicleDto.ColorID,
                LicensePlate = createVehicleDto.LicensePlate,
                VIN = createVehicleDto.VIN,
                Year = createVehicleDto.Year,
                Odometer = createVehicleDto.Odometer,
                LastServiceDate = DateTime.UtcNow, // Set default value
                CreatedAt = DateTime.UtcNow
            };
            
            var createdVehicle = await _vehicleRepository.CreateAsync(vehicle);
            return _mapper.Map<VehicleDto>(createdVehicle);
        }

        public async Task<VehicleDto> UpdateVehicleAsync(Guid vehicleId, UpdateVehicleDto updateVehicleDto)
        {
            var existingVehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (existingVehicle == null)
            {
                throw new ArgumentException($"Vehicle with ID {vehicleId} not found.");
            }

            var userVehicles = await _vehicleRepository.GetByUserIdAsync(existingVehicle.UserId);
            var normalizedLicense = (updateVehicleDto.LicensePlate ?? string.Empty).ToUpper();
            var normalizedVin = updateVehicleDto.VIN?.Trim().ToUpper();

            if (!string.IsNullOrWhiteSpace(normalizedVin) &&
                userVehicles.Any(v => v.VehicleId != existingVehicle.VehicleId && string.Equals(v.VIN ?? string.Empty, normalizedVin, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ApplicationException("VIN đã tồn tại cho một xe khác của bạn. Vui lòng kiểm tra.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedLicense) &&
                userVehicles.Any(v => v.VehicleId != existingVehicle.VehicleId && string.Equals(v.LicensePlate ?? string.Empty, normalizedLicense, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ApplicationException("Biển số đã tồn tại cho một xe khác của bạn. Vui lòng kiểm tra.");
            }

            _mapper.Map(updateVehicleDto, existingVehicle);
            existingVehicle.UpdatedAt = DateTime.UtcNow;

            var updatedVehicle = await _vehicleRepository.UpdateAsync(existingVehicle);
            return _mapper.Map<VehicleDto>(updatedVehicle);
        }

        public async Task<bool> DeleteVehicleAsync(Guid vehicleId)
        {
            var exists = await _vehicleRepository.ExistsAsync(vehicleId);
            if (!exists) return false;

            if (await _vehicleRepository.HasRepairOrdersAsync(vehicleId))
            {
                throw new Exception("Cannot delete vehicle with existing repair orders");
            }

            if (await _vehicleRepository.HasRepairRequestsAsync(vehicleId))
            {
                throw new Exception("Cannot delete vehicle with existing repair requests");
            }

            if (await _vehicleRepository.HasQuotationsAsync(vehicleId))
            {
                throw new Exception("Cannot delete vehicle with existing quotations");
            }

            if (await _vehicleRepository.HasEmergencyRequestsAsync(vehicleId))
            {
                throw new Exception("Cannot delete vehicle with existing emergency requests");
            }

            return await _vehicleRepository.DeleteAsync(vehicleId);
        }

        public async Task<bool> VehicleExistsAsync(Guid vehicleId)
        {
            return await _vehicleRepository.ExistsAsync(vehicleId);
        }

        public async Task<VehicleWithCustomerDto?> GetVehicleWithCustomerAsync(Guid vehicleId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null) return null;

            var vehicleDto = _mapper.Map<VehicleDto>(vehicle);
            var customerDto = new RoBoardCustomerDto
            {
                UserId = vehicle.User.Id,
                FullName = vehicle.User.FullName,
                Email = vehicle.User.Email,
                PhoneNumber = vehicle.User.PhoneNumber
            };

            return new VehicleWithCustomerDto
            {
                Vehicle = vehicleDto,
                Customer = customerDto
            };
        }

        public async Task<bool> UpdateWarrantyStatusAsync(Guid vehicleId, string warrantyStatus)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null) return false;

            vehicle.WarrantyStatus = warrantyStatus;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _vehicleRepository.UpdateAsync(vehicle);
            return true;
        }

        public async Task<bool> UpdateServiceScheduleAsync(Guid vehicleId, DateTime? nextServiceDate)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null) return false;

            vehicle.NextServiceDate = nextServiceDate;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _vehicleRepository.UpdateAsync(vehicle);
            return true;
        }
    }
}