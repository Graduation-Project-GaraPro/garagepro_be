using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Services.RoleServices
{
    public class PermissionService : IPermissionService
    {
        private readonly MyAppDbContext _context;

        public PermissionService(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RoleHasPermissionAsync(string roleId, string permissionCode)
        {
            return await _context.RolePermissions
                .AnyAsync(rp => rp.RoleId == roleId && rp.Permission.Code == permissionCode);
        }

        public async Task<HashSet<string>> GetPermissionsByRoleIdAsync(string roleId)
        {
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission.Code)
                .ToListAsync();

            return permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public void InvalidateRolePermissions(string roleId)
        {
            // Implementation cho bản gốc thì không cần
            // Cái này sẽ được override trong CachedPermissionService
        }
    }
}
