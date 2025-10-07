using BusinessObject.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.RepairRequestRepositories
{
    public interface IRequestPartRepository
    {
        Task<IEnumerable<RequestPart>> GetAllAsync();
        Task<IEnumerable<RequestPart>> GetByRepairRequestIdAsync(Guid repairRequestId);
        Task<RequestPart> GetByIdAsync(Guid id);
        Task<RequestPart> AddAsync(RequestPart requestPart);
        Task<RequestPart> UpdateAsync(RequestPart requestPart);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
