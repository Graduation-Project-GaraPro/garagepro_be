using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.SystemLogs;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories.LogRepositories
{
    public class SystemLogRepository : ISystemLogRepository
    {
        private readonly MyAppDbContext _context;

        public SystemLogRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SystemLog log)
        {
            _context.SystemLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SystemLog>> GetAllAsync()
        {
            return await _context.SystemLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<SystemLog>> GetRecentAsync(int count = 50)
        {
            return await _context.SystemLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();
        }
    }
}
