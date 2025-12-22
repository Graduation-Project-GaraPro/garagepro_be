using BusinessObject.Branches;
using BusinessObject.Customers;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.BranchRepositories
{
    public class BranchRepository : IBranchRepository
    {
        private readonly MyAppDbContext _context;
        public record BranchBlockInfo(Guid BranchId, bool HasActiveRequests, bool HasActiveOrders);
        public BranchRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<Branch?> GetByIdAsync(Guid id)
        {
            return await _context.Branches.FindAsync(id);
        }

        public async Task<Branch?> GetBranchWithRelationsAsync(Guid id)
        {
            return await _context.Branches
                .Include(b => b.BranchServices).ThenInclude(bs => bs.Service)
                .Include(b => b.OperatingHours)
                .Include(b => b.Staffs)
                .Include(b => b.RepairRequests)
                .AsSplitQuery()
                .FirstOrDefaultAsync(b => b.BranchId == id);
        }

        public async Task<IEnumerable<Branch>> GetAllAsync()
        {
            return await _context.Branches
               .Include(b => b.BranchServices).ThenInclude(bs => bs.Service)
               .Include(b => b.OperatingHours)
               .Include(b => b.Staffs)
               .AsSplitQuery()
               .ToListAsync();
        }
        public IQueryable<Branch> Query()
        {
            return _context.Branches.Include(b => b.BranchServices).ThenInclude(bs => bs.Service)
               .Include(b => b.OperatingHours)
               .AsSplitQuery()
               .Include(b => b.Staffs).AsQueryable();
            // có thể Include navigation nếu muốn, ví dụ:
            // return _context.Branches.Include(b => b.Services).AsQueryable();
        }
        public async Task AddAsync(Branch branch)
        {
            await _context.Branches.AddAsync(branch);
        }

        public async Task UpdateAsync(Branch branch)
        {
            _context.Branches.Update(branch);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Branch branch)
        {
            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateIsActiveForManyAsync(IEnumerable<Guid> branchIds, bool isActive)
        {
            var branches = await _context.Branches
                .Where(b => branchIds.Contains(b.BranchId))
                .ToListAsync();

            if (branches.Any())
            {
                foreach (var branch in branches)
                {
                    branch.IsActive = isActive;
                }

                await _context.SaveChangesAsync();
            }
        }
        public async Task DeleteManyAsync(IEnumerable<Guid> branchIds)
        {
            var branches = await _context.Branches
                .Where(b => branchIds.Contains(b.BranchId))
                .ToListAsync();

            if (branches.Any())
            {
                _context.Branches.RemoveRange(branches);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(List<BranchBlockInfo> blocked, List<Guid> allowed)>
    CheckBranchesCanChangeActiveAsync(IEnumerable<Guid> branchIds)
        {
            var ids = branchIds.Distinct().ToList();

            // RepairRequest hoạt động
            var activeRequestBranchIds = await _context.RepairRequests
                .AsNoTracking()
                .Where(r => ids.Contains(r.BranchId)
                    && (r.Status == RepairRequestStatus.Pending
                        || r.Status == RepairRequestStatus.Accept
                        || r.Status == RepairRequestStatus.Arrived))
                .Select(r => r.BranchId)
                .Distinct()
                .ToListAsync();

            // RepairOrder hoạt động
            var activeOrderBranchIds = await _context.RepairOrders
                .AsNoTracking()
                .Where(o => ids.Contains(o.BranchId)
                    
                    && !o.IsArchived)
                .Select(o => o.BranchId)
                .Distinct()
                .ToListAsync();

            var blockedSet = activeRequestBranchIds.Concat(activeOrderBranchIds).ToHashSet();

            var blocked = ids
                .Where(id => blockedSet.Contains(id))
                .Select(id => new BranchBlockInfo(
                    id,
                    HasActiveRequests: activeRequestBranchIds.Contains(id),
                    HasActiveOrders: activeOrderBranchIds.Contains(id)
                ))
                .ToList();

            var allowed = ids.Where(id => !blockedSet.Contains(id)).ToList();

            return (blocked, allowed);
        }

        public async Task RemoveBranchServicesAsync(Branch branch)
        {
            _context.BranchServices.RemoveRange(branch.BranchServices);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveOperatingHoursAsync(Branch branch)
        {
            _context.OperatingHours.RemoveRange(branch.OperatingHours);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<Branch, bool>> predicate)
        {
            return await _context.Branches.AnyAsync(predicate);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<BusinessObject.Authentication.ApplicationUser>> GetManagersByBranchAsync(Guid branchId)
        {
            var managerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Manager");

            if (managerRole == null) return new List<BusinessObject.Authentication.ApplicationUser>();

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == managerRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            return await _context.Users
                .Where(u => userIds.Contains(u.Id) && u.BranchId == branchId)
                .ToListAsync();
        }
    }


}
