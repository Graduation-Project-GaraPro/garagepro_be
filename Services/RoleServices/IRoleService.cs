using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Roles;
using Dtos.Auth;
using Dtos.Roles;
using Microsoft.AspNetCore.Identity;

namespace Services.RoleServices
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(string roleId);
        Task<RoleDto> UpdateRoleAsync(UpdateRoleDto dto);
        Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
        Task DeleteRoleAsync(string roleId);

        Task<List<Permission>> GetPermissionsForRoleAsync(string roleId);
        Task AssignPermissionAsync(string roleId, Guid permissionId, string grantedBy);

        Task<bool> RoleHasPermissionAsync(string roleId, string permissionCode);
        Task<List<Permission>> GetPermissionsByRoleAsync(string roleId);


        Task RemovePermissionAsync(string roleId, Guid permissionId);

        Task<List<UserDto>> GetUsersByRoleIdAsync(string roleId);
    }
}
