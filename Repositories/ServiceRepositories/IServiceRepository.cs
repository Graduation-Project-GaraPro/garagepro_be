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
        Task<Service> GetByIdAsync(Guid id);
        Task<Service?> GetByIdWithRelationsAsync(Guid id);
        IQueryable<Service> Query();
        Task AddAsync(Service service);
        void Update(Service service);
        void Delete(Service service);
        Task SaveChangesAsync();
    }
}
