using Dtos.Vehicles;
using Repositories.VehicleRepositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;

namespace Services.VehicleServices
{
    public class VehicleColorService : IVehicleColorService
    {
        private readonly IVehicleColorRepository _vehicleColorRepository;
        private readonly IMapper _mapper;

        public VehicleColorService(IVehicleColorRepository vehicleColorRepository, IMapper mapper)
        {
            _vehicleColorRepository = vehicleColorRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VehicleColorDto>> GetVehicleColorsAsync()
        {
            var colors = await _vehicleColorRepository.GetAllAsync(); // trả về List<VehicleColor>
            return _mapper.Map<IEnumerable<VehicleColorDto>>(colors); // map sang DTO
        }

        public async Task<VehicleColorDto> GetColorByIdAsync(Guid id)
        {
            var color = await _vehicleColorRepository.GetByIdAsync(id);
            if (color == null) return null;

            return _mapper.Map<VehicleColorDto>(color);
        }
        public async Task<List<VehicleColorDto>> GetColorsByModelAsync(Guid modelId)
        {
            // Lấy danh sách VehicleModelColor theo modelId
            var modelColors = await _vehicleColorRepository.GetColorsByModelIdAsync(modelId);

            // Map sang DTO
            return _mapper.Map<List<VehicleColorDto>>(modelColors);
        }

    }
}
