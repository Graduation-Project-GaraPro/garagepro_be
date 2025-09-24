//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BusinessObject.SystemLogs;
//using Repositories;

//namespace Services
//{
//    public class SystemLogService : ISystemLogService
//    {
//        private readonly ISystemLogRepository _repository;

//        public SystemLogService(ISystemLogRepository repository)
//        {
//            _repository = repository;
//        }

//        public async Task<IEnumerable<SystemLog>> GetLogsAsync(LogLevel? level, DateTimeOffset? start, DateTimeOffset? end, string? userId)
//        {
//            return await _repository.GetAllAsync(level, start, end, userId);
//        }

//        public async Task<SystemLog?> GetLogAsync(long id)
//        {
//            return await _repository.GetByIdAsync(id);
//        }

//        public async Task<SystemLog> CreateLogAsync(SystemLog log)
//        {
//            log.Timestamp = DateTimeOffset.UtcNow;
//            await _repository.AddAsync(log);
//            await _repository.SaveChangesAsync();
//            return log;
//        }

//        public async Task<bool> DeleteLogAsync(long id)
//        {
//            var log = await _repository.GetByIdAsync(id);
//            if (log == null) return false;

//            await _repository.DeleteAsync(log);
//            await _repository.SaveChangesAsync();
//            return true;
//        }
//    }
//}
