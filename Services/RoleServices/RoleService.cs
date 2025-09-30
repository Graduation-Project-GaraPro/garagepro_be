using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Roles;
using Microsoft.AspNetCore.Identity;
using Repositories.RoleRepositories;

namespace Services.RoleServices
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepo;
        private readonly IRolePermissionRepository _rolePermissionRepo;
        private readonly IPermissionService _permissionService;
        public RoleService(IRoleRepository roleRepo, IRolePermissionRepository rolePermissionRepo, IPermissionService permissionService)
        {
            _roleRepo = roleRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _permissionService = permissionService;
        }

        public Task<List<ApplicationRole>> GetAllRolesAsync() => _roleRepo.GetAllRolesAsync();
        public Task<ApplicationRole?> GetRoleByIdAsync(string roleId) => _roleRepo.GetRoleByIdAsync(roleId);
        public async Task<ApplicationRole> CreateRoleAsync(string name, string description = "", bool isDefault = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name cannot be empty", nameof(name));

            var role = new ApplicationRole
            {
                Name = name,
                NormalizedName = name.ToUpperInvariant(),
                Description = description,
                IsDefault = isDefault,
                CreatedAt = DateTime.UtcNow,
                Users=0
            };

            var result = await _roleRepo.CreateRoleAsync(role);

            if (result == null)
                throw new InvalidOperationException($"Failed to create role {name}");

            return result;
        }

        public async Task UpdateRoleAsync(string roleId, string newName, string? description = null)
        {
            var role = await _roleRepo.GetRoleByIdAsync(roleId);
            if (role == null) throw new KeyNotFoundException("Role not found.");

            if (!string.IsNullOrWhiteSpace(newName))
            {
                role.Name = newName;
                role.NormalizedName = newName.ToUpperInvariant();
            }

            if (description != null)
                role.Description = description;

            role.UpdatedAt = DateTime.UtcNow;

            await _roleRepo.UpdateRoleAsync(role);
        }

        public Task DeleteRoleAsync(string roleId) => _roleRepo.DeleteRoleAsync(roleId);



        public async Task<bool> RoleHasPermissionAsync(string roleId, string permissionCode)
        {
            var permissions = await _rolePermissionRepo.GetByRoleIdAsync(roleId);
            return permissions.Any(p => p.Code == permissionCode);
        }

        public async Task<List<Permission>> GetPermissionsByRoleAsync(string roleId)
        {
            return await _rolePermissionRepo.GetPermissionsForRoleAsync(roleId);
        }


        public Task<List<Permission>> GetPermissionsForRoleAsync(string roleId)
            => _rolePermissionRepo.GetPermissionsForRoleAsync(roleId);

        public async Task AssignPermissionAsync(string roleId, Guid permissionId, string grantedBy)
        {
            await _rolePermissionRepo.AssignPermissionAsync(roleId, permissionId, grantedBy);
            _permissionService.InvalidateRolePermissions(roleId);
        }

        public async Task RemovePermissionAsync(string roleId, Guid permissionId)
        {
            await _rolePermissionRepo.RemovePermissionAsync(roleId, permissionId);
            _permissionService.InvalidateRolePermissions(roleId);
        }
    }
}
