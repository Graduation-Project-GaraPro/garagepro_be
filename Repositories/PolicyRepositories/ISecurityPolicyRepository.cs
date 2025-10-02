using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Policies;
using Microsoft.EntityFrameworkCore.Storage;

namespace Repositories.PolicyRepositories
{
    public interface ISecurityPolicyRepository
    {
        Task<SecurityPolicy?> GetCurrentAsync();
        Task UpdateAsync(SecurityPolicy policy);
        Task AddHistoryAsync(SecurityPolicyHistory history);
        Task<IEnumerable<SecurityPolicyHistory>> GetAllHistoryAsync();
        Task<(IEnumerable<SecurityPolicyHistory> Items, int TotalCount)> GetAuditHistoryAsync(
        int page, int pageSize, string? search, string? changedBy, DateTime? dateFrom, DateTime? dateTo);
        Task<SecurityPolicyHistory> GetHistoryAsync(Guid historyId);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task SaveChangesAsync();
    }
}
