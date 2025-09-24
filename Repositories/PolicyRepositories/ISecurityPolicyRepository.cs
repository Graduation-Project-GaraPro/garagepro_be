using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Policies;

namespace Repositories.PolicyRepositories
{
    public interface ISecurityPolicyRepository
    {
        Task<SecurityPolicy?> GetCurrentAsync();
        Task UpdateAsync(SecurityPolicy policy);
        Task AddHistoryAsync(SecurityPolicyHistory history);
        Task SaveChangesAsync();
    }
}
