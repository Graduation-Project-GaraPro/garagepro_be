using BusinessObject.Vehicles;
using Dtos.Vehicles;
using Repositories.Vehicles;
using Services.VehicleServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Vehicles
{
    public class VehicleBrandService : IVehicleBrandServices
    {
        private readonly IVehicleBrandRepository _repository;

        public VehicleBrandService(IVehicleBrandRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<VehicleBrandDto>> GetAllBrandsAsync()
        {
            var brands = await _repository.GetAllAsync();
            return brands.Select(b => new VehicleBrandDto
            {
                BrandID = b.BrandID,
                BrandName = b.BrandName,
                Country = b.Country,
                CreatedAt = b.CreatedAt,
                Models = b.VehicleModels.Select(m => new VehicleModelDto
                {
                    ModelID = m.ModelID,
                    ModelName = m.ModelName,
                    ManufacturingYear = m.ManufacturingYear,
                    BrandID = m.BrandID,
                    BrandName = b.BrandName,
                    CreatedAt = m.CreatedAt
                }).ToList()
            });
        }

        public async Task<VehicleBrandDto> GetBrandByIdAsync(Guid id)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null) return null;

            return new VehicleBrandDto
            {
                BrandID = brand.BrandID,
                BrandName = brand.BrandName,
                Country = brand.Country,
                CreatedAt = brand.CreatedAt,
                Models = brand.VehicleModels.Select(m => new VehicleModelDto
                {
                    ModelID = m.ModelID,
                    ModelName = m.ModelName,
                    ManufacturingYear = m.ManufacturingYear,
                    BrandID = m.BrandID,
                    BrandName = brand.BrandName,
                    CreatedAt = m.CreatedAt
                }).ToList()
            };
        }

        public async Task<VehicleBrandDto> CreateBrandAsync(CreateVehicleBrandDto brandDto)
        {
            var brand = new VehicleBrand
            {
                BrandID = Guid.NewGuid(),
                BrandName = brandDto.BrandName,
                Country = brandDto.Country,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(brand);

            return new VehicleBrandDto
            {
                BrandID = brand.BrandID,
                BrandName = brand.BrandName,
                Country = brand.Country,
                CreatedAt = brand.CreatedAt,
                Models = new List<VehicleModelDto>()
            };
        }

        //public async Task<bool> UpdateBrandAsync(Guid id, UpdateVehicleBrandDto brandDto)
        //{
        //    var brand = await _repository.GetByIdAsync(id);
        //    if (brand == null) return false;

        //    brand.BrandName = brandDto.BrandName;
        //    brand.Country = brandDto.Country;

        //    return await _repository.UpdateAsync(brand);
        //}

        //public async Task<bool> DeleteBrandAsync(Guid id)
        //{
        //    var brand = await _repository.GetByIdAsync(id);
        //    if (brand == null) return false;

        //    var hasVehicles = await _repository.HasVehiclesAsync(id);
        //    if (hasVehicles) return false;

        //    return await _repository.DeleteAsync(brand);
        //}
    }
}
