using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Services.LogServices;
using Services.UserServices;

namespace Garage_pro_api.DbInterceptor
{
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceProvider _serviceProvider;

        public AuditSaveChangesInterceptor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            var logService = _serviceProvider.GetRequiredService<ILogService>();
            var currentUser = _serviceProvider.GetRequiredService<ICurrentUserService>();

            var user = currentUser?.UserName ?? "Unknown";
            var context = eventData.Context;

            foreach (var entry in context.ChangeTracker.Entries()
                         .Where(e => e.State == EntityState.Added
                                  || e.State == EntityState.Modified
                                  || e.State == EntityState.Deleted))
            {
                var entityName = entry.Entity.GetType().Name;
                var action = entry.State.ToString();

                //logService.Info($"User '{user}' performed {action} on {entityName}");
                logService.LogDatabaseAsync(action, entityName,LogLevel.Information);
            }

            return base.SavingChanges(eventData, result);
        }
    }
}
