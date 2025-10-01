using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Roles;

namespace Repositories.RoleRepositories
{
    public interface IRolePermissionRepository
    {
        Task<List<Permission>> GetPermissionsForRoleAsync(string roleId);
        Task AssignPermissionAsync(string roleId, Guid permissionId, string grantedBy, string grantedUserId = null);
        Task<List<Permission>> GetByRoleIdAsync(string roleId);

        Task RemoveAllPermissionsAsync(string roleId);
        Task AssignPermissionsToRoleAsync(string roleId, List<Guid> permissionIds);
        Task RemovePermissionAsync(string roleId, Guid permissionId);
    }
}
