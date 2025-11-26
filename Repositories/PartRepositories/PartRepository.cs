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

        public IQueryable<Part> Query()
        {
            return _context.Parts
                .Include(p => p.PartCategory)
                
                    
                .AsQueryable();
        }

        // Get Part by ID
        public async Task<Part> GetByIdAsync(Guid id)
        {
            return await _context.Parts.FindAsync(id);
        }

        // Get all parts
        public async Task<IEnumerable<Part>> GetAllAsync()
        {
            return await _context.Parts.ToListAsync();
        }

        // Create a new part
        public async Task<Part> CreateAsync(Part part)
        {
            _context.Parts.Add(part);
            await _context.SaveChangesAsync();
            return part;
        }

        // Update an existing part
        public async Task<Part> UpdateAsync(Part part)
        {
            _context.Parts.Update(part);
            await _context.SaveChangesAsync();
            return part;
        }

        // Delete a part
        public async Task<bool> DeleteAsync(Guid id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null)
                return false;

            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}