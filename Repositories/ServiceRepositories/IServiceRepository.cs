using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;

namespace Repositories.ServiceRepositories
{
    public interface IServiceRepository
    {
        Task<IEnumerable<Service>> GetAllAsync();
    }
}
