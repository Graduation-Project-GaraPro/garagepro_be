using Dtos.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.VehicleServices
{
   
        public interface IVehicleBrandServices
        {
            Task<IEnumerable<VehicleBrandDto>> GetAllBrandsAsync();
            Task<VehicleBrandDto> GetBrandByIdAsync(Guid id);
            Task<VehicleBrandDto> CreateBrandAsync(CreateVehicleBrandDto brandDto);
            //Task<bool> UpdateBrandAsync(Guid id, UpdateVehicleBrandDto brandDto);
            //Task<bool> DeleteBrandAsync(Guid id);
        }
    }

