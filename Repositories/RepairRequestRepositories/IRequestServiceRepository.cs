using BusinessObject.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.RepairRequestRepositories
{
    public interface IRequestServiceRepository
    {
        Task<IEnumerable<RequestService>> GetAllAsync();
        Task<IEnumerable<RequestService>> GetByRepairRequestIdAsync(Guid repairRequestId);
        Task<RequestService> GetByIdAsync(Guid id);
        Task<RequestService> AddAsync(RequestService requestService);
        Task<RequestService> UpdateAsync(RequestService requestService);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);

    }
}
