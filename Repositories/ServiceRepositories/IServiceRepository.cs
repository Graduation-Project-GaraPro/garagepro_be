using BusinessObject;
using BusinessObject.Branches;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.ServiceRepositories
{
    public interface IServiceRepository
    {
        Task<IEnumerable<Service>> GetAllAsync();
        Task<Service> GetByIdAsync(Guid id);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<Service?> GetByIdWithRelationsAsync(Guid id);
        IQueryable<Service> Query();
        Task AddAsync(Service service);
        void Update(Service service);
        void Delete(Service service);
        void DeleteServiceCategoryRange(List<ServicePartCategory> servicePartCategory);
        void DeleteBranchServicesRange(List<BranchService> BranchServices);
        Task SaveChangesAsync();
    }
}
