using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.RoleServices
{
    public interface IPermissionService
    {
        Task<bool> RoleHasPermissionAsync(string roleId, string permissionCode);

        Task<HashSet<string>> GetPermissionsByRoleIdAsync(string roleId);

        void InvalidateRolePermissions(string roleId);
    }
}
