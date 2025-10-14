//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BusinessObject.SystemLogs;

//namespace Repositories
//{
//    public interface ISystemLogRepository
//    {
//        Task<IEnumerable<SystemLog>> GetAllAsync(
//            LogLevel? level = null,
//            DateTimeOffset? start = null,
//            DateTimeOffset? end = null,
//            string? userId = null);

//        Task<SystemLog?> GetByIdAsync(long id);
//        Task AddAsync(SystemLog log);
//        Task DeleteAsync(SystemLog log);
//        Task SaveChangesAsync();
//    }
//}
