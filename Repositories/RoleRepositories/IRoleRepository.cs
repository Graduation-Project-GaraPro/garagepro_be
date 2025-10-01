using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;
using BusinessObject.Roles;
using Microsoft.AspNetCore.Identity;

namespace Repositories.RoleRepositories
{
    public interface IRoleRepository
    {
        Task<List<ApplicationRole>> GetAllRolesAsync();
        Task<ApplicationRole?> GetRoleByIdAsync(string roleId);
        Task<ApplicationRole> CreateRoleAsync(ApplicationRole role);
        Task UpdateRoleAsync(ApplicationRole role);

        Task DeleteRoleAsync(string roleId);

        Task<List<ApplicationUser>> GetUsersByRoleIdAsync(string roleId);
    }
}
