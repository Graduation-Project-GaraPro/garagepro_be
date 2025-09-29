using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Services.PolicyServices
{
    public class RealTimePasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : ApplicationUser
    {
        private readonly ISecurityPolicyService _securityPolicyService;

        public RealTimePasswordValidator(ISecurityPolicyService securityPolicyService)
        {
            _securityPolicyService = securityPolicyService;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            var result = await _securityPolicyService.ValidatePasswordAsync(password);

            if (result.IsValid)
                return IdentityResult.Success;

            var identityErrors = result.Errors.Select(error => new IdentityError
            {
                Code = "PasswordPolicyViolation",
                Description = error
            });

            return IdentityResult.Failed(identityErrors.ToArray());
        }
    }
}
