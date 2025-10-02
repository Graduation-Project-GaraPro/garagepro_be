using BusinessObject.Authentication;
using BusinessObject.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Services.RoleServices;

namespace Garage_pro_api.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IPermissionService _permissionService;

        public PermissionHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IPermissionService permissionService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // 1. Lấy user hiện tại
            var user = await _userManager.GetUserAsync(context.User);
            if (user == null) return;

            // 2. Lấy tất cả roles của user
            var roleNames = await _userManager.GetRolesAsync(user);

            foreach (var roleName in roleNames)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                // 3. Kiểm tra nếu role có đủ tất cả quyền
                bool hasAllPermissions = true;
                foreach (var code in requirement.PermissionCodes)
                {
                    if (!await _permissionService.RoleHasPermissionAsync(role.Id, code))
                    {
                        hasAllPermissions = false;
                        break;
                    }
                }

                if (hasAllPermissions)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
            // Nếu không role nào đủ quyền => Authorization fail
        }
    }
}
