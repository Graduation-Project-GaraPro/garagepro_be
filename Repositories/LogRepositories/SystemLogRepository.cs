using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.SystemLogs;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<SystemLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.SystemLogs
                
                .Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<SystemLog>> GetBySourceAsync(LogSource source)
        {
            return await _context.SystemLogs
                .Where(x => x.Source == source)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<SystemLog>> GetByUserIdAsync(string userId)
        {
            return await _context.SystemLogs              
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<SystemLog>> GetByLevelAsync(LogLevel level)
        {
            return await _context.SystemLogs               
                .Where(x => x.Level == level)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }
    }
}
