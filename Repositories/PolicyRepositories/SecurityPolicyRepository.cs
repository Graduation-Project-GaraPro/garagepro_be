using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Policies;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
            _db.Entry(policy).Property(x => x.UpdatedAt).IsModified = true;
            await Task.CompletedTask;
        }

        public async Task AddHistoryAsync(SecurityPolicyHistory history)
        {
            await _db.SecurityPolicyHistories.AddAsync(history);
        }
        public async Task<SecurityPolicyHistory> GetHistoryAsync(Guid historyId)
        {
            return await _db.SecurityPolicyHistories
                .FirstOrDefaultAsync(ht => ht.HistoryId == historyId)
                ;
        }


        public async Task<IEnumerable<SecurityPolicyHistory>> GetAllHistoryAsync()
        {
            return await _db.SecurityPolicyHistories

                .Include(h => h.ChangedByUser)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();
        }
        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<(IEnumerable<SecurityPolicyHistory> Items, int TotalCount)> GetAuditHistoryAsync(
             int page, int pageSize, string? search, string? changedBy, DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _db.SecurityPolicyHistories
                .Include(h => h.Policy)
                .Include(h => h.ChangedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(changedBy))
                query = query.Where(h => h.ChangedBy == changedBy);

            if (dateFrom.HasValue)
                query = query.Where(h => h.ChangedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(h => h.ChangedAt <= dateTo.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(h =>
                    (h.ChangeSummary != null && EF.Functions.Like(h.ChangeSummary, $"%{search}%")) ||
                    (h.PreviousValues != null && EF.Functions.Like(h.PreviousValues, $"%{search}%")) ||
                    (h.NewValues != null && EF.Functions.Like(h.NewValues, $"%{search}%")));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(h => h.ChangedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _db.Database.BeginTransactionAsync();
        }
    }
}
