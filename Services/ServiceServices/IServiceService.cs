using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;

namespace Services.ServiceServices
{
    public interface IServiceService
    {
        Task<IEnumerable<Service>> GetAllServicesAsync();
    }
}
