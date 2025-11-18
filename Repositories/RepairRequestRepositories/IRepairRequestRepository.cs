using BusinessObject.Customers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repositories.Customers
{
    public interface IRepairRequestRepository
    {
        Task<IEnumerable<RepairRequest>> GetAllAsync();
        Task<IEnumerable<RepairRequest>> GetByUserIdAsync(String userId);
        Task<IEnumerable<RepairRequest>> GetByBranchIdAsync(Guid branchId); // New method
        Task<RepairRequest> GetByIdAsync(Guid id);
        Task<int> CountAsync(Expression<Func<RepairRequest, bool>> predicate);
        Task<IEnumerable<RepairRequest>> ListByConditionAsync(Expression<Func<RepairRequest, bool>> predicate);

        Task<bool> AnyAsync(Expression<Func<RepairRequest, bool>> predicate);
        Task<RepairRequest> GetTrackingByIdAsync(Guid id);
        Task<RepairRequest> GetByIdWithDetailsAsync(Guid id); // New method for managers
        Task<RepairRequest> AddAsync(RepairRequest repairRequest);
        Task<bool> UpdateAsync(RepairRequest repairRequest);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);

        Task<RepairRequest?> GetByEmergencyIdAsync(Guid emergencyId);

        IQueryable<RepairRequest> GetQueryable();

    }
}