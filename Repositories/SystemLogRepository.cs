//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BusinessObject.SystemLogs;
//using DataAccessLayer;

//namespace Repositories
//{
//    public class SystemLogRepository : ISystemLogRepository
//    {
//        private readonly MyAppDbContext _context;

//        public SystemLogRepository(MyAppDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IEnumerable<SystemLog>> GetAllAsync(
//            LogLevel? level = null,
//            DateTimeOffset? start = null,
//            DateTimeOffset? end = null,
//            string? userId = null)
//        {
//            var query = _context.SystemLogs
//                .Include(l => l.)
//                .Include(l => l.Tags)
//                .Include(l => l.User)
//                .AsQueryable();

//            if (level.HasValue)
//                query = query.Where(l => l.Level == level);

//            if (start.HasValue)
//                query = query.Where(l => l.Timestamp >= start.Value);

//            if (end.HasValue)
//                query = query.Where(l => l.Timestamp <= end.Value);

//            if (!string.IsNullOrEmpty(userId))
//                query = query.Where(l => l.UserId == userId);

//            return await query.OrderByDescending(l => l.Timestamp).ToListAsync();
//        }

//        public async Task<SystemLog?> GetByIdAsync(long id)
//        {
//            return await _context.SystemLogs
//                .Include(l => l.Category)
//                .Include(l => l.Tags)
//                .Include(l => l.User)
//                .FirstOrDefaultAsync(l => l.Id == id);
//        }

//        public async Task AddAsync(SystemLog log)
//        {
//            await _context.SystemLogs.AddAsync(log);
//        }

//        public async Task DeleteAsync(SystemLog log)
//        {
//            _context.SystemLogs.Remove(log);
//        }

//        public async Task SaveChangesAsync()
//        {
//            await _context.SaveChangesAsync();
//        }
//    }
//}
