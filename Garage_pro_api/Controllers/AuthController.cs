using BusinessObject.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Services;
using Dtos.Auth;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

            // Assign default role
            await _userManager.AddToRoleAsync(user, "Customer");

            return Ok(new { Message = "User registered successfully" });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return BadRequest(new { Error = "Account locked out due to multiple failed login attempts" });

            if (!result.Succeeded)
                return BadRequest(new { Error = "Invalid login attempt" });

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Generate JWT token
            var token = await _tokenService.GenerateJwtToken(user);

            // Set refresh token in cookie if needed
            Response.Cookies.Append("X-Refresh-Token", token, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresIn = Convert.ToInt32(TimeSpan.FromMinutes(30).TotalSeconds),
                UserId = user.Id,
                Email = user.Email,
                Roles = await _userManager.GetRolesAsync(user)
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete("X-Refresh-Token");

            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["X-Refresh-Token"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            try
            {
                var principal = _tokenService.ValidateToken(refreshToken);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                    return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return Unauthorized();

                var newToken = await _tokenService.GenerateJwtToken(user);

                return Ok(new AuthResponseDto
                {
                    Token = newToken,
                    ExpiresIn = Convert.ToInt32(TimeSpan.FromMinutes(30).TotalSeconds)
                });
            }
            catch (SecurityTokenException)
            {
                return Unauthorized();
            }
        }
    }
}
