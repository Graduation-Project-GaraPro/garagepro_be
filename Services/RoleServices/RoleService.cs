using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.Authentication;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IPermissionService _permissionService;
        private readonly IHubContext<PermissionHub> _permissionHub;
        private readonly IMapper _mapper;
        public RoleService(MyAppDbContext context, IHubContext<PermissionHub> permissionHub,IRoleRepository roleRepo, 
            IRolePermissionRepository rolePermissionRepo, IPermissionService permissionService,
             UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMapper mapper)
        {
            _roleRepo = roleRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _permissionService = permissionService;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
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

                // B1: validate permission IDs sent from client
                var existingPermissions = await _permissionService.Query()
                    .Where(p => dto.PermissionIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                var invalidPermissions = dto.PermissionIds.Except(existingPermissions).ToList();
                if (invalidPermissions.Any())
                {
                    throw new ApplicationException(
                        $"The following permissions do not exist: {string.Join(", ", invalidPermissions)}");
                }

                // B2: auto-append default permissions by category
                var normalizedPermissionIds = await NormalizePermissionIdsWithDefaultsAsync(existingPermissions);

                // NEW: do not allow assigning system permissions through this API
                var systemPermissionIds = await _permissionService.Query()
                    .Where(p => normalizedPermissionIds.Contains(p.Id) && p.IsSystem)
                    .Select(p => p.Id)
                    .ToListAsync();

                if (systemPermissionIds.Any())
                {
                    // Bạn có thể log thêm list ID nếu muốn debug
                    throw new InvalidOperationException(
                        "System permissions cannot be assigned through this API.");
                }

                // B3: assign valid + normalized permissions
                foreach (var permissionId in normalizedPermissionIds)
                {
                    await _rolePermissionRepo.AssignPermissionAsync(
                        createdRole.Id,
                        permissionId,
                        dto.GrantedBy,
                        dto.GrantedUserId
                    );
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

                if (string.Equals(role.Name, "Customer", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Customer role is system role and cannot be updated.");
                }
                if (string.Equals(role.Name, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Admin role is system role and cannot be updated.");
                }

                // Lấy các permission hiện có của role, để check system
                var currentPermissions = await _permissionService.Query()
                    .Where(p => p.RolePermissions.Any(rp => rp.RoleId == role.Id))
                    .Select(p => new { p.Id, p.IsSystem })
                    .ToListAsync();

                var currentSystemPermissionIds = currentPermissions
                    .Where(x => x.IsSystem)
                    .Select(x => x.Id)
                    .ToHashSet();

                if (!role.IsDefault)
                {
                    role.Name = dto.Name;
                    role.NormalizedName = dto.Name.ToUpperInvariant();
                }

                if (dto.Description != null)
                    role.Description = dto.Description;

                role.UpdatedAt = DateTime.UtcNow;
                await _roleRepo.UpdateRoleAsync(role);

                // B1: validate permissionId
                var existingPermissions = await _permissionService.Query()
                    .Where(p => dto.PermissionIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                var invalidPermissions = dto.PermissionIds.Except(existingPermissions).ToList();
                if (invalidPermissions.Any())
                {
                    throw new ApplicationException(
                        $"The following permissions do not exist: {string.Join(", ", invalidPermissions)}");
                }

                // B2: bổ sung default permission theo Category
                var normalizedPermissionIds = await NormalizePermissionIdsWithDefaultsAsync(existingPermissions);

                // B3: check System permissions không bị thay đổi
                var targetSystemPermissionIds = await _permissionService.Query()
                    .Where(p => normalizedPermissionIds.Contains(p.Id) && p.IsSystem)
                    .Select(p => p.Id)
                    .ToListAsync();

                var removedSystemPermissions = currentSystemPermissionIds
                    .Except(targetSystemPermissionIds)
                    .ToList();

                if (removedSystemPermissions.Any())
                {
                    throw new InvalidOperationException(
                                 "System permissions cannot be removed from this role."
                             );
                }

                var addedSystemPermissions = targetSystemPermissionIds
                    .Except(currentSystemPermissionIds)
                    .ToList();

                if (addedSystemPermissions.Any())
                {
                    throw new InvalidOperationException(
                            "System permissions cannot be assigned to this role."
                        );
                }

                
                await _rolePermissionRepo.RemoveAllPermissionsAsync(role.Id);

                foreach (var permissionId in normalizedPermissionIds)
                {
                    await _rolePermissionRepo.AssignPermissionAsync(
                        role.Id,
                        permissionId,
                        dto.GrantedBy,
                        dto.GrantedUserId
                    );
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


        public async Task AssignRoleToUsersAsync(AssignRoleToUsersDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.RoleId))
                throw new ArgumentException("RoleId is required", nameof(dto.RoleId));

            if (dto.UserIds == null || !dto.UserIds.Any())
                throw new ArgumentException("At least one user must be specified", nameof(dto.UserIds));

            // Lấy role theo Id
            var role = await _roleManager.FindByIdAsync(dto.RoleId);
            if (role == null)
                throw new KeyNotFoundException("Role not found.");

            // Nếu bạn có rule: không gán được role Customer cho internal user, vv... thì check ở đây

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var userId in dto.UserIds.Distinct())
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        //_logger.LogWarning("User {UserId} not found when assigning role {RoleName}", userId, role.Name);
                        continue; // hoặc throw, tuỳ bạn muốn strict hay không
                    }

                    // Lấy tất cả role hiện tại của user
                    var currentRoles = await _userManager.GetRolesAsync(user);

                    // Nếu đã có đúng 1 role và chính là role này rồi thì bỏ qua
                    if (currentRoles.Count == 1 && currentRoles.Contains(role.Name))
                    {
                        continue;
                    }

                    // Xoá tất cả role hiện tại (đảm bảo "chỉ 1 role")
                    if (currentRoles.Any())
                    {
                        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        if (!removeResult.Succeeded)
                        {
                            var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                            throw new ApplicationException($"Failed to remove old roles from user {user.Id}: {errors}");
                        }
                    }

                    // Thêm role mới
                    var addResult = await _userManager.AddToRoleAsync(user, role.Name);
                    if (!addResult.Succeeded)
                    {
                        var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                        throw new ApplicationException($"Failed to add role {role.Name} to user {user.Id}: {errors}");
                    }

                    
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
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


        private async Task<List<Guid>> NormalizePermissionIdsWithDefaultsAsync(IEnumerable<Guid> rawPermissionIds)
        {
            var permissionIds = rawPermissionIds.Distinct().ToList();

            if (!permissionIds.Any())
                return permissionIds;

            // Lấy thông tin Category của các permission được chọn
            var permissionsWithCategory = await _permissionService.Query()
                .Where(p => permissionIds.Contains(p.Id))
                .Select(p => new { p.Id, p.CategoryId })
                .ToListAsync();

            var categoryIds = permissionsWithCategory
                .Select(p => p.CategoryId)
                .Distinct()
                .ToList();

            if (!categoryIds.Any())
                return permissionIds;

            // Lấy các permission IsDefault trong các Category đó
            var defaultPermissions = await _permissionService.Query()
                .Where(p => categoryIds.Contains(p.CategoryId) && p.IsDefault && !p.Deprecated)
                .Select(p => p.Id)
                .ToListAsync();

            // Thêm default permission vào danh sách nếu chưa có
            var result = new HashSet<Guid>(permissionIds);
            foreach (var defaultId in defaultPermissions)
            {
                result.Add(defaultId);
            }

            return result.ToList();
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
