using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Roles;
using Dtos.Roles;

namespace Services.RoleServices
{
    public interface IPermissionService
    {
        Task<bool> RoleHasPermissionAsync(string roleId, string permissionCode);

        Task<List<PermissionCategoryDto>> GetAllPermissionsGroupedAsync();

        Task<List<Permission>> GetAllPermissionsAsync();
        Task<HashSet<string>> GetPermissionsByRoleIdAsync(string roleId);

        void InvalidateRolePermissions(string roleId);
    }
}
