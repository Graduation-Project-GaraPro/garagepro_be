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
    public class OperatingHourRepository : IOperatingHourRepository
    {
        private readonly MyAppDbContext _context;

        public OperatingHourRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OperatingHour>> GetByBranchAsync(Guid branchId)
        {
            return await _context.OperatingHours
                .Where(o => o.BranchId == branchId)
                .ToListAsync();
        }

        public async Task<OperatingHour> SingleOrDefaultAsync(Expression<Func<OperatingHour, bool>> predicate)
        {
            return await _context.OperatingHours
                .SingleOrDefaultAsync(predicate);
        }



    }

}
