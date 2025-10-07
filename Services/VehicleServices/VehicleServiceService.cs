using AutoMapper;
using BusinessObject;
using BusinessObject.Vehicles;
using Dtos.Vehicles;
using Repositories;
using Repositories.BranchRepositories;
using Repositories.VehicleRepositories;
using Repositories.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Vehicles
{
    public class VehicleServiceService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IVehicleBrandRepository _brandRepository;
        private readonly IVehicleModelRepository _modelRepository;
        private readonly IVehicleColorRepository _colorRepository;
        private readonly IUserRepository _userRepository;

        private readonly IMapper _mapper;

        public VehicleServiceService(IVehicleRepository vehicleRepository, IMapper mapper, IVehicleBrandRepository brandRepository, IVehicleModelRepository modelRepository, IVehicleColorRepository colorRepository,IUserRepository userRepository)
        {
            _vehicleRepository = vehicleRepository;
            _mapper = mapper;
            _brandRepository = brandRepository;
            _modelRepository = modelRepository;
            _colorRepository = colorRepository;
            _userRepository = userRepository;


        }

        public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync()
        {
            var vehicles = await _vehicleRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
        }

        public async Task<IEnumerable<VehicleDto>> GetUserVehiclesAsync(String userId)
        {
            var vehicles = await _vehicleRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
        }

        public async Task<VehicleDto> GetVehicleByIdAsync(Guid id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);

            if (vehicle == null)
                throw new KeyNotFoundException($"Vehicle with ID {id} was not found.");

            return _mapper.Map<VehicleDto>(vehicle);
        }


        public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto vehicleDto, String UserId)
        {
            //  Validate selected options
            if (!await _brandRepository.ExistsAsync(vehicleDto.BrandID))
                throw new ArgumentException("Selected brand does not exist.");

            if (!await _modelRepository.ExistsAsync(vehicleDto.ModelID))
                throw new ArgumentException("Selected model does not exist.");

            if (!await _colorRepository.ExistsAsync(vehicleDto.ColorID))
                throw new ArgumentException("Selected color does not exist.");

            if (await _userRepository.GetByIdAsync(UserId)!= null)
                throw new ArgumentException("User does not exist.");
            //Create and save the vehicle
            var vehicle = new Vehicle
            {
                VehicleId = Guid.NewGuid(),
                BrandId = vehicleDto.BrandID,
                UserId = UserId,
                ModelId = vehicleDto.ModelID,
                ColorId = vehicleDto.ColorID,
                LicensePlate = vehicleDto.LicensePlate,
                VIN = vehicleDto.VIN,
                
                CreatedAt = DateTime.UtcNow
            };

            await _vehicleRepository.AddAsync(vehicle);
            return await GetVehicleByIdAsync(vehicle.VehicleId);
        }

        public async Task<VehicleDto> UpdateVehicleAsync(Guid id, UpdateVehicleDto vehicleDto)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null)
                return null;

            vehicle.BrandId = vehicleDto.BrandID;
            vehicle.ModelId = vehicleDto.ModelID;
            vehicle.ColorId = vehicleDto.ColorID;
            vehicle.LicensePlate = vehicleDto.LicensePlate;
            vehicle.VIN = vehicleDto.VIN;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _vehicleRepository.UpdateAsync(vehicle);
            return await GetVehicleByIdAsync(id);
        }

        public async Task<bool> DeleteVehicleAsync(Guid id)
        {
            return await _vehicleRepository.DeleteAsync(id);
        }
    }
}