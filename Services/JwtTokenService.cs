using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using BusinessObject;
using BusinessObject.Authentication;
using Services.PolicyServices;

namespace Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISecurityPolicyService _policyService;
        public JwtTokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager, ISecurityPolicyService policyService)
        {
            _configuration = configuration;
            _userManager = userManager;
            _policyService = policyService;
        }

        public async Task<string> GenerateAccessToken(ApplicationUser user)
        {
            var policy = await _policyService.GetCurrentAsync();

            var pwdChangedAt = user.LastPasswordChangeDate ?? user.CreatedAt; // fallback hợp lý
            var pwdExpireAt = pwdChangedAt.AddDays(policy.PasswordExpiryDays);
            return await GenerateJwtTokenCore(user, policy.SessionTimeout, policyUpdatedAtUtc: policy.UpdatedAt, tokenType: "access", pwdExpireAt);
        }

        public async Task<string> GenerateRefreshToken(ApplicationUser user, int refreshLifetimeMinutes)
        {
            var policy = await _policyService.GetCurrentAsync();
            // vẫn gắn policyUpdatedAt để đồng bộ revoke theo policy khi cần

            var pwdChangedAt = user.LastPasswordChangeDate ?? user.CreatedAt; // fallback hợp lý
            var pwdExpireAt = pwdChangedAt.AddDays(policy.PasswordExpiryDays);
            return await GenerateJwtTokenCore(user, refreshLifetimeMinutes, policyUpdatedAtUtc: policy.UpdatedAt, tokenType: "refresh", pwdExpireAt);
        }

        private async Task<string> GenerateJwtTokenCore(ApplicationUser user, int lifetimeMinutes, DateTime policyUpdatedAtUtc, string tokenType, DateTime pwdExpireAt)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
            var now = DateTime.UtcNow;



            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Id),
            new Claim("FirstName", user.FirstName ?? string.Empty),
            new Claim("LastName",  user.LastName  ?? string.Empty),

            // KHÔNG thêm thuộc tính mới vào SecurityPolicy: tận dụng UpdatedAt
            // để biết token phát hành trước/sau lần cập nhật policy gần nhất
            new Claim("policyUpdatedAt", policyUpdatedAtUtc.Ticks.ToString()),
            new Claim("pwd_exp_at",pwdExpireAt.Ticks.ToString()),
           
            new Claim("typ", tokenType) // "access" | "refresh"
        };

            // Roles
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

            // Extra user claims (permissions, ...)
            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = now,
                Expires = now.AddMinutes(lifetimeMinutes),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, parameters, out _);
        }

        

        
    }
}
