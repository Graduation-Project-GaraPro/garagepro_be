using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Branches;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories.BranchRepositories
{
    public class BranchRepository : IBranchRepository
    {
        private readonly MyAppDbContext _context;

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
                .FirstOrDefaultAsync(b => b.BranchId == id);
        }

        public async Task<IEnumerable<Branch>> GetAllAsync()
        {
            return await _context.Branches
               .Include(b => b.BranchServices).ThenInclude(bs => bs.Service)
               .Include(b => b.OperatingHours)
               .Include(b => b.Staffs)
               .ToListAsync();
        }
        public IQueryable<Branch> Query()
        {
            return _context.Branches.Include(b => b.BranchServices).ThenInclude(bs => bs.Service)
               .Include(b => b.OperatingHours)
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
    }


}
