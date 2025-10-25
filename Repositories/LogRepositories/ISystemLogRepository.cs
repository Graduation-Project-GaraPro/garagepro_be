using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.SystemLogs;

namespace Repositories.LogRepositories
{
    public interface ISystemLogRepository
    {
        Task AddAsync(SystemLog log);
        Task<IEnumerable<SystemLog>> GetAllAsync();
        Task<IEnumerable<SystemLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<SystemLog>> GetBySourceAsync(LogSource source);
        Task<IEnumerable<SystemLog>> GetByUserIdAsync(string userId);
    }
}
