using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Roles;
using DataAccessLayer;
using Dtos.Roles;
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
        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            return await _context.Permissions
                .Include(p => p.Category)  // load Category cho permission
                .ToListAsync();
        }


        public async Task<List<PermissionCategoryDto>> GetAllPermissionsGroupedAsync()
        {
            var categories = await _context.PermissionCategories
                .Include(c => c.Permissions)
                .ToListAsync();

            return categories.Select(c => new PermissionCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Permissions = c.Permissions.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Description = p.Description,
                    Deprecated = p.Deprecated
                }).ToList()
            }).ToList();
        }

        public void InvalidateRolePermissions(string roleId)
        {
            // Implementation cho bản gốc thì không cần
            // Cái này sẽ được override trong CachedPermissionService
        }
    }
}
