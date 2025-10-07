using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories.PartRepositories
{
    public class PartRepository : IPartRepository
    {
        private readonly MyAppDbContext _context;

        public PartRepository(MyAppDbContext context)
        {
            _context = context;
        }

        // Check if Part exists by predicate
        public async Task<bool> ExistsAsync(Expression<Func<Part, bool>> predicate)
        {
            return await _context.Parts.AnyAsync(predicate);
        }
    }
}
