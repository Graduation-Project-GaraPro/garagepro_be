using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Roles;
using Microsoft.AspNetCore.Identity;

namespace Services.RoleServices
{
    public interface IRoleService
    {
        Task<List<ApplicationRole>> GetAllRolesAsync();
        Task<ApplicationRole?> GetRoleByIdAsync(string roleId);
        Task<ApplicationRole> CreateRoleAsync(string name, string description = "", bool isDefault = false);
        Task UpdateRoleAsync(string roleId, string newName, string? description = null);
        Task DeleteRoleAsync(string roleId);

        Task<List<Permission>> GetPermissionsForRoleAsync(string roleId);
        Task AssignPermissionAsync(string roleId, Guid permissionId, string grantedBy);

        Task<bool> RoleHasPermissionAsync(string roleId, string permissionCode);
        Task<List<Permission>> GetPermissionsByRoleAsync(string roleId);


        Task RemovePermissionAsync(string roleId, Guid permissionId);
    }
}
