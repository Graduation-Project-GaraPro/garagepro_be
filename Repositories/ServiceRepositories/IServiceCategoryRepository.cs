using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;

namespace Repositories.ServiceRepositories
{
    public interface IServiceCategoryRepository
    {
        Task<IEnumerable<ServiceCategory>> GetAllAsync();
        Task<ServiceCategory> GetByIdAsync(Guid id);
        Task<IEnumerable<Service>> GetServicesByCategoryIdAsync(Guid categoryId);
    }
}
