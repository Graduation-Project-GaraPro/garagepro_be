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
        Task<string> GenerateJwtToken(ApplicationUser user, int time);
        ClaimsPrincipal ValidateToken(string token);
    }
}
