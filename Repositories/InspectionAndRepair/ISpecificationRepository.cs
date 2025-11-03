using BusinessObject.InspectionAndRepair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.InspectionAndRepair
{
    public interface ISpecificationRepository
    {
        Task<List<VehicleLookup>> GetAllSpecificationsAsync();
        Task<List<VehicleLookup>> SearchSpecificationsAsync(string keyword);
    }
}
