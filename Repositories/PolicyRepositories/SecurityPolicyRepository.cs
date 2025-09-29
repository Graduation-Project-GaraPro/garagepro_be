using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Policies;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories.PolicyRepositories
{
    public class SecurityPolicyRepository : ISecurityPolicyRepository
    {
        private readonly MyAppDbContext _db;
        public SecurityPolicyRepository(MyAppDbContext db) => _db = db;

        public async Task<SecurityPolicy?> GetCurrentAsync()
        {
            // Nếu bạn chắc chỉ có 1 record, lấy FirstOrDefault
            return await _db.SecurityPolicies.FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(SecurityPolicy policy)
        {
            _db.SecurityPolicies.Update(policy);
            await Task.CompletedTask;
        }

        public async Task AddHistoryAsync(SecurityPolicyHistory history)
        {
            await _db.SecurityPolicyHistories.AddAsync(history);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
