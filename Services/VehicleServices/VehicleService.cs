using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject;
using Dtos.Vehicle;
using Repositories.VehicleRepositories;

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

        public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto createVehicleDto)
        {
            var vehicle = _mapper.Map<Vehicle>(createVehicleDto);
            vehicle.VehicleId = Guid.NewGuid();
            vehicle.CreatedAt = DateTime.UtcNow;
            
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

            _mapper.Map(updateVehicleDto, existingVehicle);
            existingVehicle.UpdatedAt = DateTime.UtcNow;

            var updatedVehicle = await _vehicleRepository.UpdateAsync(existingVehicle);
            return _mapper.Map<VehicleDto>(updatedVehicle);
        }

        public async Task<bool> DeleteVehicleAsync(Guid vehicleId)
        {
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
            var customerDto = _mapper.Map<CustomerDto>(vehicle.User);

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