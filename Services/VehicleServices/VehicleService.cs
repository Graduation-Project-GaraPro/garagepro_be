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
            if (vehiclesExits.Any(v => v.VIN == vin))
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
                throw new ArgumentException($"Vehicle with ID {vehicleId} not found.");

            var userVehicles = await _vehicleRepository.GetByUserIdAsync(existingVehicle.UserId);

            // Normalize input
            var normalizedLicense = (updateVehicleDto.LicensePlate ?? string.Empty).Trim().ToUpperInvariant();
            var normalizedVin = (updateVehicleDto.VIN ?? string.Empty).Trim().ToUpperInvariant();

            // Check VIN duplicate only if VIN is provided
            if (!string.IsNullOrWhiteSpace(normalizedVin))
            {
                bool vinExists = userVehicles.Any(v =>
                    v.VehicleId != existingVehicle.VehicleId &&
                    !string.IsNullOrWhiteSpace(v.VIN) &&
                    string.Equals(v.VIN.Trim(), normalizedVin, StringComparison.OrdinalIgnoreCase)
                );

                if (vinExists)
                    throw new ApplicationException("VIN already exists for another vehicle. Please check.");
            }

            // Check License Plate duplicate only if license is provided
            if (!string.IsNullOrWhiteSpace(normalizedLicense))
            {
                bool licenseExists = userVehicles.Any(v =>
                    v.VehicleId != existingVehicle.VehicleId &&
                    !string.IsNullOrWhiteSpace(v.LicensePlate) &&
                    string.Equals(v.LicensePlate.Trim(), normalizedLicense, StringComparison.OrdinalIgnoreCase)
                );

                if (licenseExists)
                    throw new ApplicationException("License plate already exists for another vehicle. Please check.");
            }

            // Map updates
            _mapper.Map(updateVehicleDto, existingVehicle);

            // Apply normalized values only if provided
            if (!string.IsNullOrWhiteSpace(normalizedVin))
                existingVehicle.VIN = normalizedVin;

            if (!string.IsNullOrWhiteSpace(normalizedLicense))
                existingVehicle.LicensePlate = normalizedLicense;

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
                FirstName = vehicle.User.FirstName,
                LastName = vehicle.User.LastName,
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

        public async Task<VehicleDto> CreateVehicleForCustomerAsync(CreateVehicleForCustomerDto createVehicleDto)
        {
            var vehiclesExits = await _vehicleRepository.GetByUserIdAsync(createVehicleDto.CustomerUserId);
            var vin = createVehicleDto.VIN;
            
            if (!string.IsNullOrWhiteSpace(vin) && vehiclesExits.Any(v => v.VIN == vin))
            {
                throw new Exception("VIN already exists for this customer");
            }

            if (vehiclesExits.Any(v => v.LicensePlate == createVehicleDto.LicensePlate))
            {
                throw new Exception("License plate already exists for this customer");
            }

            var vehicle = new Vehicle
            {
                BrandId = createVehicleDto.BrandID,
                UserId = createVehicleDto.CustomerUserId,
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
    }
}