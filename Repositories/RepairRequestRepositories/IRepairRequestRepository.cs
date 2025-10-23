using BusinessObject.Customers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Customers
{
    public interface IRepairRequestRepository
    {
        Task<IEnumerable<RepairRequest>> GetAllAsync();
        Task<IEnumerable<RepairRequest>> GetByUserIdAsync(String userId);
        Task<RepairRequest> GetByIdAsync(Guid id);
        Task<RepairRequest> GetTrackingByIdAsync(Guid id);
        Task<RepairRequest> AddAsync(RepairRequest repairRequest);
        Task<bool> UpdateAsync(RepairRequest repairRequest);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);

        
        IQueryable<RepairRequest> GetQueryable();

    }
}