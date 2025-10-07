using AutoMapper;
using Dtos.Vehicles;
using Repositories.Vehicles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.VehicleServices
{
    public class VehicleModelService : IVehicleModelService
    {
        private readonly IVehicleModelRepository _vehicleModelRepository;
        private readonly IMapper _mapper;

        public VehicleModelService(IVehicleModelRepository vehicleModelRepository, IMapper mapper)
        {
            _vehicleModelRepository = vehicleModelRepository;
            _mapper = mapper;
        }

        public async Task<List<VehicleModelDto>> GetAllVehicleModelsAsync()
        {
            var models = await _vehicleModelRepository.GetAllAsync(); 
            return _mapper.Map<List<VehicleModelDto>>(models);       // map sang DTO
        }

        public async Task<VehicleModelDto> GetVehicleModelByIdAsync(Guid modelId)
        {
            var model = await _vehicleModelRepository.GetByIdAsync(modelId);
            if (model == null) return null;

            return _mapper.Map<VehicleModelDto>(model);
        }
        public async Task<List<VehicleModelDto>> GetModelsByBrandAsync(Guid brandId)
        {
            var models = await _vehicleModelRepository.GetByBrandIdAsync(brandId);
            return _mapper.Map<List<VehicleModelDto>>(models);
        }

    }
}
