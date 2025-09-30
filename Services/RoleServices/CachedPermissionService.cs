using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Services.RoleServices
{
    public class CachedPermissionService : IPermissionService
    {
        private readonly IPermissionService _innerService;
        private readonly IMemoryCache _cache;

        public CachedPermissionService(IPermissionService innerService, IMemoryCache cache)
        {
            _innerService = innerService;
            _cache = cache;
        }

        public async Task<bool> RoleHasPermissionAsync(string roleId, string permissionCode)
        {
            var permissions = await GetPermissionsByRoleIdAsync(roleId);
            return permissions.Contains(permissionCode);
        }

        public async Task<HashSet<string>> GetPermissionsByRoleIdAsync(string roleId)
        {
            var cacheKey = $"role_permissions_{roleId}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return await _innerService.GetPermissionsByRoleIdAsync(roleId);
            });
        }

        public void InvalidateRolePermissions(string roleId)
        {
            var cacheKey = $"role_permissions_{roleId}";
            _cache.Remove(cacheKey);
        }
    }
}
