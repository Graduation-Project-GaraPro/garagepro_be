using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.Roles;
using Dtos.Auth;
using Dtos.Roles;
using Microsoft.AspNetCore.Identity;
using Repositories.RoleRepositories;

namespace Services.RoleServices
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepo;
        private readonly IRolePermissionRepository _rolePermissionRepo;
        private readonly IPermissionService _permissionService;
        private readonly IMapper _mapper;
        public RoleService(IRoleRepository roleRepo, IRolePermissionRepository rolePermissionRepo, IPermissionService permissionService, IMapper mapper)
        {
            _roleRepo = roleRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _permissionService = permissionService;
            _mapper = mapper;
        }


        private async Task<RoleDto> MapRoleWithPermissions(ApplicationRole role)
        {
            var permissions = await _rolePermissionRepo.GetPermissionsForRoleAsync(role.Id);

            // group theo category
            var grouped = permissions
                .GroupBy(p => p.Category)
                .Select(g =>
                {
                    var categoryDto = _mapper.Map<PermissionCategoryDto>(g.Key);
                    categoryDto.Permissions = _mapper.Map<List<PermissionDto>>(g.ToList());
                    return categoryDto;
                }).ToList();

            var dto = _mapper.Map<RoleDto>(role);
            dto.PermissionCategories = grouped;
            return dto;
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepo.GetAllRolesAsync();
            var result = new List<RoleDto>();

            foreach (var role in roles)
                result.Add(await MapRoleWithPermissions(role));

            return result;
        }

        public async Task<RoleDto?> GetRoleByIdAsync(string roleId)
        {
            var role = await _roleRepo.GetRoleByIdAsync(roleId);
            return role == null ? null : await MapRoleWithPermissions(role);
        }


        public async Task<List<UserDto>> GetUsersByRoleIdAsync(string roleId)
        {
            var users = await _roleRepo.GetUsersByRoleIdAsync(roleId);

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id, 
                Name = $"{u.FirstName} {u.LastName}".Trim(),
                Status = u.IsActive,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber
            }).ToList();

            return userDtos;
        }



        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Role name cannot be empty", nameof(dto.Name));

            var role = new ApplicationRole
            {
                Name = dto.Name,
                NormalizedName = dto.Name.ToUpperInvariant(),
                Description = dto.Description,
                IsDefault = dto.IsDefault,
                CreatedAt = DateTime.UtcNow,
                Users = 0
            };

            var createdRole = await _roleRepo.CreateRoleAsync(role);
            if (createdRole == null)
                throw new InvalidOperationException($"Failed to create role {dto.Name}");

            foreach (var permissionId in dto.PermissionIds)
                await _rolePermissionRepo.AssignPermissionAsync(createdRole.Id, permissionId, dto.GrantedBy,dto.GrantedUserId);

            _permissionService.InvalidateRolePermissions(createdRole.Id);

            return await MapRoleWithPermissions(createdRole);
        }

        public async Task<RoleDto> UpdateRoleAsync(UpdateRoleDto dto)
        {
            var role = await _roleRepo.GetRoleByIdAsync(dto.RoleId);
            if (role == null)
                throw new KeyNotFoundException("Role not found.");

            if (!string.IsNullOrWhiteSpace(dto.NewName))
            {
                role.Name = dto.NewName;
                role.NormalizedName = dto.NewName.ToUpperInvariant();
            }
            if (dto.Description != null)
                role.Description = dto.Description;

            role.UpdatedAt = DateTime.UtcNow;
            await _roleRepo.UpdateRoleAsync(role);

            await _rolePermissionRepo.RemoveAllPermissionsAsync(role.Id);
            foreach (var permissionId in dto.PermissionIds)
                await _rolePermissionRepo.AssignPermissionAsync(role.Id, permissionId, dto.GrantedBy, dto.GrantedUserId);

            _permissionService.InvalidateRolePermissions(role.Id);

            return await MapRoleWithPermissions(role);
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
