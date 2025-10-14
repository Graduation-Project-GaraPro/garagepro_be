using Dtos.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.VehicleServices
{
    public interface IVehicleColorService
    {
        Task<IEnumerable<VehicleColorDto>> GetVehicleColorsAsync();
        Task<VehicleColorDto> GetColorByIdAsync(Guid id);
        Task<List<VehicleColorDto>> GetColorsByModelAsync(Guid ModelId);
        //Task<VehicleBrandDto> CreateBraAsync(CreateVehicleBrandDto brandDto);
    }
}
