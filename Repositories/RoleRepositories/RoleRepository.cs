using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;
using BusinessObject.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Repositories.RoleRepositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleRepository(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<List<ApplicationRole>> GetAllRolesAsync()
            => _roleManager.Roles.Include(r=>r.RolePermissions).ThenInclude(rp=>rp.Permission).ThenInclude(p=>p.Category).ToList();

        public async Task<ApplicationRole?> GetRoleByIdAsync(string roleId)
            => await _roleManager.FindByIdAsync(roleId);

        public async Task<ApplicationRole> CreateRoleAsync(ApplicationRole role)
        {
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            return role;
        }

        public async Task UpdateRoleAsync(ApplicationRole role)
        {
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task DeleteRoleAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Lấy tất cả User thuộc role
        public async Task<List<ApplicationUser>> GetUsersByRoleIdAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                throw new Exception($"Role with id {roleId} not found.");

            var users = await _userManager.GetUsersInRoleAsync(role.Name);
            return users.ToList();
        }
    }
}
