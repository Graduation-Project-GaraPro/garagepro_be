using Microsoft.AspNetCore.Authorization;

namespace Garage_pro_api.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public IReadOnlyCollection<string> PermissionCodes { get; }

        public PermissionRequirement(params string[] permissionCodes)
        {
            PermissionCodes = permissionCodes;
        }
    }
}
