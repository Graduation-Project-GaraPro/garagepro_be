using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Roles;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories.RoleRepositories
{
    public class RolePermissionRepository : IRolePermissionRepository
    {
        private readonly MyAppDbContext _context;

        public RolePermissionRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Permission>> GetPermissionsForRoleAsync(string roleId)
            => await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission)
                .ToListAsync();

        public async Task<List<Permission>> GetByRoleIdAsync(string roleId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task AssignPermissionAsync(string roleId, Guid permissionId, string grantedBy)
        {
            var exists = await _context.RolePermissions
                .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
            if (!exists)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    GrantedBy = grantedBy,
                    GrantedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }
        public async Task AssignPermissionsToRoleAsync(string roleId, List<Guid> permissionIds)
        {
            var existing = _context.RolePermissions.Where(rp => rp.RoleId == roleId);
            _context.RolePermissions.RemoveRange(existing);

            var newRolePermissions = permissionIds.Select(pid => new RolePermission
            {
                RoleId = roleId,
                PermissionId = pid
            });

            await _context.RolePermissions.AddRangeAsync(newRolePermissions);
            await _context.SaveChangesAsync();
        }
        public async Task RemovePermissionAsync(string roleId, Guid permissionId)
        {
            var entity = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
            if (entity != null)
            {
                _context.RolePermissions.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
