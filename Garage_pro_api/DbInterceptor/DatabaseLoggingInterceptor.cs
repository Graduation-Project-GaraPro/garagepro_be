using Microsoft.EntityFrameworkCore.Diagnostics;
using Services.LogServices;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;

namespace Garage_pro_api.DbInterceptor
{
    public class DatabaseLoggingInterceptor : DbCommandInterceptor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly int _slowQueryThresholdMs;
        private static readonly ConcurrentDictionary<Guid, Stopwatch> _stopwatches = new();

        public DatabaseLoggingInterceptor(IServiceProvider serviceProvider, int slowQueryThresholdMs = 1000)
        {
            _serviceProvider = serviceProvider;
            _slowQueryThresholdMs = slowQueryThresholdMs;
        }

        // === READER SYNC ===
        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            _stopwatches[eventData.CommandId] = Stopwatch.StartNew();
            return base.ReaderExecuting(command, eventData, result);
        }

        public override DbDataReader ReaderExecuted(
            DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        {
            if (_stopwatches.TryRemove(eventData.CommandId, out var sw))
            {
                sw.Stop();
                if (sw.ElapsedMilliseconds > _slowQueryThresholdMs)
                {
                    LogDatabaseOperation("SELECT", command.CommandText, sw.ElapsedMilliseconds);
                }
            }
            return base.ReaderExecuted(command, eventData, result);
        }

        // === READER ASYNC ===
        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command, CommandExecutedEventData eventData, DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            if (_stopwatches.TryRemove(eventData.CommandId, out var sw))
            {
                sw.Stop();
                if (sw.ElapsedMilliseconds > _slowQueryThresholdMs)
                {
                    LogDatabaseOperation("SELECT", command.CommandText, sw.ElapsedMilliseconds);
                }
            }
            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        // === NON-QUERY SYNC ===
        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            _stopwatches[eventData.CommandId] = Stopwatch.StartNew();
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override int NonQueryExecuted(
            DbCommand command, CommandExecutedEventData eventData, int result)
        {
            if (_stopwatches.TryRemove(eventData.CommandId, out var sw))
            {
                sw.Stop();
                if (sw.ElapsedMilliseconds > _slowQueryThresholdMs)
                {
                    var operation = GetOperationType(command.CommandText);
                    LogDatabaseOperation(operation, command.CommandText, sw.ElapsedMilliseconds);
                }
            }
            return base.NonQueryExecuted(command, eventData, result);
        }

        // === NON-QUERY ASYNC ===
        public override async ValueTask<int> NonQueryExecutedAsync(
            DbCommand command, CommandExecutedEventData eventData, int result,
            CancellationToken cancellationToken = default)
        {
            if (_stopwatches.TryRemove(eventData.CommandId, out var sw))
            {
                sw.Stop();
                if (sw.ElapsedMilliseconds > _slowQueryThresholdMs)
                {
                    var operation = GetOperationType(command.CommandText);
                    LogDatabaseOperation(operation, command.CommandText, sw.ElapsedMilliseconds);
                }
            }
            return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        // === LOGGING ===
        private void LogDatabaseOperation(string operation, string commandText, long elapsedMs)
        {
            try
            {
                var tableName = GetTableName(commandText);
                if (!string.IsNullOrEmpty(tableName))
                {
                    _ = Task.Run(() => LogInBackgroundScope(operation, tableName, commandText, elapsedMs));
                }
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private async Task LogInBackgroundScope(string operation, string tableName, string commandText, long elapsedMs)
        {
            using var scope = _serviceProvider.CreateScope();
            try
            {
                var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
                await logService.LogDatabaseAsync(
                    operation,
                    tableName,
                    details: $"⚠️ Slow query detected ({elapsedMs} ms)\nCommand: {commandText}"
                );
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private string GetOperationType(string commandText)
        {
            var upperCmd = commandText.ToUpperInvariant();
            if (upperCmd.Contains("INSERT")) return "INSERT";
            if (upperCmd.Contains("UPDATE")) return "UPDATE";
            if (upperCmd.Contains("DELETE")) return "DELETE";
            return "SELECT";
        }

        private string? GetTableName(string commandText)
        {
            var upperCmd = commandText.ToUpperInvariant();
            if (upperCmd.Contains("FROM"))
            {
                var fromIndex = upperCmd.IndexOf("FROM") + 5;
                var nextSpace = upperCmd.IndexOf(' ', fromIndex);
                if (nextSpace == -1) nextSpace = upperCmd.Length;
                return upperCmd[fromIndex..nextSpace].Trim().Replace("[", "").Replace("]", "");
            }
            return null;
        }
    }
}
