using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Azure;
using BusinessObject.Authentication;
using BusinessObject.Policies;
using Dtos.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Services.EmailSenders;
using Services.PolicyServices;

namespace Services.Authentication
{
    public class DynamicAuthenticationService 
    {
        private readonly ISecurityPolicyService _securityPolicyService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMemoryCache _cache;
        

        public DynamicAuthenticationService(
            ISecurityPolicyService securityPolicyService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IMemoryCache cache)
        {
            _securityPolicyService = securityPolicyService;
            _userManager = userManager;
            _signInManager = signInManager;
            _cache = cache;
            
        }

        public async Task<SignInResult> PasswordSignInAsync(string email, string password, bool isPersistent)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return SignInResult.Failed;
            var policy = await _securityPolicyService.GetCurrentAsync();

            // Kiểm tra lockout real-time
            if (policy?.EnableBruteForceProtection == true && await IsAccountLockedOutAsync(user))
                return SignInResult.LockedOut;

            // Thực hiện đăng nhập
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                await ResetFailedAccessCountAsync(user);

               
                if (policy != null)
                {
                    await SetAuthenticationCookieAsync(user, isPersistent, policy.SessionTimeout);
                }

                return SignInResult.Success;
            }
            else if (result.IsLockedOut)
            {
                return SignInResult.LockedOut;
            }
            else
            {
                if (policy?.EnableBruteForceProtection == true)
                    await HandleFailedLoginAsync(user, policy);
                return SignInResult.Failed;
            }
        }
        public async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal)
        {
            return await _userManager.GetUserAsync(principal);
        }
        public async Task<bool> IsAccountLockedOutAsync(ApplicationUser user)
        {
            var policy = await _securityPolicyService.GetCurrentAsync();
            if (policy == null || !policy.EnableBruteForceProtection)
                return false;

            // Kiểm tra lockout từ Identity
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            if (lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.UtcNow)
                return true;

            return false;
        }

        private async Task HandleFailedLoginAsync(ApplicationUser user, SecurityPolicy policy)
        {
            if (policy == null || !policy.EnableBruteForceProtection)
                return;

            // Tăng FailedCount trong Identity
            await _userManager.AccessFailedAsync(user);

            var failedCount = await _userManager.GetAccessFailedCountAsync(user);
            if (failedCount >= policy.MaxLoginAttempts)
            {
                var lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(policy.AccountLockoutTime);
                await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            }
        }


        public async Task<LockoutInfo> GetLockoutInfoAsync(ApplicationUser user)
        {
            if (user == null) return new LockoutInfo { IsLockedOut = false };

            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            return new LockoutInfo
            {
                IsLockedOut = lockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = lockoutEnd,
                RemainingMinutes = lockoutEnd.HasValue ?
                    (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes : 0
            };
        }
        private async Task ResetFailedAccessCountAsync(ApplicationUser user)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        private async Task SetAuthenticationCookieAsync(ApplicationUser user, bool isPersistent, int sessionTimeout)
        {
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = isPersistent ?
                    DateTimeOffset.UtcNow.AddDays(30) :
                    DateTimeOffset.UtcNow.AddMinutes(sessionTimeout)
            };

            await _signInManager.Context.SignInAsync(principal, authProperties);
        }

        public async Task<bool> RequiresMfaAsync(ApplicationUser user)
        {
            var policy = await _securityPolicyService.GetCurrentAsync();
            return policy?.MfaRequired == true;
        }

        public async Task<bool> IsPasswordExpiredAsync(ApplicationUser user)
        {
            var policy = await _securityPolicyService.GetCurrentAsync();
            if (policy == null || policy.PasswordExpiryDays <= 0)
                return false;

            var lastPasswordChange = await GetLastPasswordChangeDateAsync(user);
            if (!lastPasswordChange.HasValue)
                return true;

            return lastPasswordChange.Value.AddDays(policy.PasswordExpiryDays) < DateTime.UtcNow;
        }

        public async Task UpdateLastPasswordChangeAsync(ApplicationUser user)
        {
            user.LastPasswordChangeDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        private async Task<DateTime?> GetLastPasswordChangeDateAsync(ApplicationUser user)
        {
            if (user == null) return null;
            return user.LastPasswordChangeDate;
        }
    }
    public class LockoutInfo
    {
        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int RemainingMinutes { get; set; }
    }
}

