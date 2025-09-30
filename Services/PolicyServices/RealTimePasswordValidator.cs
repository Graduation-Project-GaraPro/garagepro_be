using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Services.PolicyServices
{
    public class RealTimePasswordValidator<TUser> : IPasswordValidator<TUser>
    where TUser : ApplicationUser
    {
        private readonly ISecurityPolicyService _securityPolicyService;

        public RealTimePasswordValidator(ISecurityPolicyService securityPolicyService)
        {
            _securityPolicyService = securityPolicyService;
        }

        public async Task<IdentityResult> ValidateAsync(
             UserManager<TUser> manager,
             TUser user,
             string password)
        {
            // Gọi service để validate password theo custom security policy
            var result = await _securityPolicyService.ValidatePasswordAsync(password);

            if (result.IsValid)
            {
                // Password hợp lệ
                return IdentityResult.Success;
            }

            // Gom tất cả thông báo lỗi từ service
            var allErrors = result.Errors
                .SelectMany(kvp => kvp.Value)
                .ToList();

            // Convert sang IdentityError để Identity xử lý chuẩn
            var identityErrors = allErrors
                .Select(msg => new IdentityError
                {
                    Code = nameof(RealTimePasswordValidator<TUser>), // hoặc "PasswordPolicyViolation"
                    Description = msg
                })
                .ToArray();

            return IdentityResult.Failed(identityErrors);
        }
    }
    }
