using BusinessObject.Technician;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Technician
{
    public interface ISpecificationService
    {
        Task<List<VehicleLookup>> GetAllSpecificationsAsync();
        Task<List<VehicleLookup>> SearchSpecificationsAsync(string keyword);
    }
}
