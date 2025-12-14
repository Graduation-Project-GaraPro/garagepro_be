using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Branches;

namespace Repositories.BranchRepositories
{
    public interface IBranchRepository
    {

        Task<bool> ExistsAsync(Expression<Func<Branch, bool>> predicate);
        IQueryable<Branch> Query();
        Task<Branch?> GetByIdAsync(Guid id);
        Task<Branch?> GetBranchWithRelationsAsync(Guid id);
        Task<IEnumerable<Branch>> GetAllAsync();
        Task AddAsync(Branch branch);
        Task UpdateAsync(Branch branch);
        Task UpdateIsActiveForManyAsync(IEnumerable<Guid> branchIds, bool isActive);
        Task DeleteAsync(Branch branch);
        Task DeleteManyAsync(IEnumerable<Guid> branchIds);
        Task RemoveBranchServicesAsync(Branch branch);
        Task RemoveOperatingHoursAsync(Branch branch);
        Task SaveChangesAsync();
        Task<List<BusinessObject.Authentication.ApplicationUser>> GetManagersByBranchAsync(Guid branchId);
    }

}
