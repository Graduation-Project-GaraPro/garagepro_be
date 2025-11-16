using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.Roles;
using DataAccessLayer;
using Dtos.Auth;
using Dtos.Roles;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Repositories.RoleRepositories;
using Services.Hubs;

namespace Services.RoleServices
{
    public class RoleService : IRoleService
    {
        private readonly MyAppDbContext _context;
        private readonly IRoleRepository _roleRepo;
        private readonly IRolePermissionRepository _rolePermissionRepo;
        private readonly IPermissionService _permissionService;
        private readonly IHubContext<PermissionHub> _permissionHub;
        private readonly IMapper _mapper;
        public RoleService(MyAppDbContext context, IHubContext<PermissionHub> permissionHub,IRoleRepository roleRepo, IRolePermissionRepository rolePermissionRepo, IPermissionService permissionService, IMapper mapper)
        {
            _roleRepo = roleRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _permissionService = permissionService;
            _mapper = mapper;
            _permissionHub = permissionHub;
            _context = context;
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
            {
                role.Users = _roleRepo.GetUsersByRoleIdAsync(role.Id).Result.Count();
                result.Add(await MapRoleWithPermissions(role));
                
            }

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


            var userDtos = _mapper.Map<List<UserDto>>(users);

            return userDtos;
        }



        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Role name cannot be empty", nameof(dto.Name));

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var role = new ApplicationRole
                {
                    Name = dto.Name,
                    NormalizedName = dto.Name.ToUpperInvariant(),
                    Description = dto.Description,
                    IsDefault = false,
                    CreatedAt = DateTime.UtcNow,
                    Users = 0
                };

                var createdRole = await _roleRepo.CreateRoleAsync(role);
                if (createdRole == null)
                    throw new InvalidOperationException($"Failed to create role {dto.Name}");

                // Lấy danh sách permission hợp lệ
                var existingPermissions = await _permissionService.Query()
                    .Where(p => dto.PermissionIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                var invalidPermissions = dto.PermissionIds.Except(existingPermissions).ToList();
                if (invalidPermissions.Any())
                {
                    throw new ApplicationException($"The following permissions do not exist: {string.Join(", ", invalidPermissions)}");
                }

                // Gán permission hợp lệ
                foreach (var permissionId in existingPermissions)
                {
                    await _rolePermissionRepo.AssignPermissionAsync(createdRole.Id, permissionId, dto.GrantedBy, dto.GrantedUserId);
                }

                _permissionService.InvalidateRolePermissions(createdRole.Id);

                await tx.CommitAsync();

                return await MapRoleWithPermissions(createdRole);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<RoleDto> UpdateRoleAsync(UpdateRoleDto dto)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var role = await _roleRepo.GetRoleByIdAsync(dto.RoleId);
                if (role == null)
                    throw new KeyNotFoundException("Role not found.");
                if(!role.IsDefault)
                {
                    role.Name = dto.Name;
                    role.NormalizedName = dto.Name.ToUpperInvariant();
                }    
                
                if (dto.Description != null)
                    role.Description = dto.Description;

                role.UpdatedAt = DateTime.UtcNow;
                await _roleRepo.UpdateRoleAsync(role);

                // Lấy danh sách permission hợp lệ
                var existingPermissions = await _permissionService.Query()
                    .Where(p => dto.PermissionIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                var invalidPermissions = dto.PermissionIds.Except(existingPermissions).ToList();
                if (invalidPermissions.Any())
                {
                    throw new ApplicationException($"The following permissions do not exist: {string.Join(", ", invalidPermissions)}");
                }

                // Xóa tất cả permission cũ
                await _rolePermissionRepo.RemoveAllPermissionsAsync(role.Id);

                // Gán permission hợp lệ
                foreach (var permissionId in existingPermissions)
                {
                    await _rolePermissionRepo.AssignPermissionAsync(role.Id, permissionId, dto.GrantedBy, dto.GrantedUserId);
                }

                _permissionService.InvalidateRolePermissions(role.Id);

                await tx.CommitAsync();

                await _permissionHub.Clients.Group(role.Name)
                .SendAsync("PermissionsUpdated", new
                {
                    role = role.Name,
                    roleId = role.Id
                });

                return await MapRoleWithPermissions(role);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        public async Task DeleteRoleAsync(string roleId)
        {
            var role = await _roleRepo.GetRoleByIdAsync(roleId);
            var userInRole = await _roleRepo.GetUsersByRoleIdAsync(roleId);

            if (role == null)
                throw new KeyNotFoundException("Role not found.");

            if (role.IsDefault)
                throw new ApplicationException("Cannot delete default role.");

            if (userInRole.Count > 0 )
                throw new ApplicationException("Cannot delete role because it has assigned users.");

            await _roleRepo.DeleteRoleAsync(roleId);
        }



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
