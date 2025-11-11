using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;

namespace Services
{
    public interface ITokenService
    {
        Task<string> GenerateAccessToken(ApplicationUser user);
        Task<string> GenerateRefreshToken(ApplicationUser user, int refreshLifetimeMinutes);
        ClaimsPrincipal ValidateToken(string token);
    }
}
