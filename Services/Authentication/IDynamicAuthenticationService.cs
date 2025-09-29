using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Services.Authentication
{
    public interface IDynamicAuthenticationService
    {
        
        Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent);
        Task<bool> IsAccountLockedOutAsync(ApplicationUser user);
        Task<bool> RequiresMfaAsync(ApplicationUser user);
        Task<bool> IsPasswordExpiredAsync(ApplicationUser user);
    }
}
