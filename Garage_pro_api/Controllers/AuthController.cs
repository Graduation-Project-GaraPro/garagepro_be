using BusinessObject.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Services;
using Dtos.Auth;
using Services.EmailSenders;
using Services.Authentication;
using Azure.Core;
using System.Data;
using System.Net;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailSender _emailSender;
        private readonly DynamicAuthenticationService _authService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,IEmailSender emailSender, DynamicAuthenticationService dynamicAuthenticationService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailSender = emailSender;
            _authService = dynamicAuthenticationService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ✅ GIỜ ĐÂY Identity sẽ tự động gọi RealTimePasswordValidator
            // Không cần validate thủ công nữa!

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest(new { error = "Email already exists" });

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.UtcNow
            };

            // ✅ CreateAsync sẽ tự động trigger password validator
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

            // Assign default role
            await _userManager.AddToRoleAsync(user, "Customer");

            // Send confirmation email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);

            var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

            await _emailSender.SendEmailAsync(
                user.Email,
                "Confirm your email",
                $@"<p>Hello {user.FirstName}, please confirm your email.</p>
           <p><a href='{confirmationLink}'>Confirm Email</a></p>"
            );

            return Ok(new
            {
                Message = "User registered successfully. Please check your email to confirm your account.",
                UserId = user.Id
            });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe);

            if (result.IsLockedOut)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                var lockoutInfo = await _authService.GetLockoutInfoAsync(user);

                return BadRequest(new
                {
                    error = "Account is temporarily locked due to failed login attempts",
                    lockoutEnd = lockoutInfo.LockoutEnd,
                    remainingMinutes = lockoutInfo.RemainingMinutes
                });
            }   

            if (!result.Succeeded)
                return BadRequest(new { error = "Invalid login attempt" });

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                    return BadRequest(new { error = "User not found" });
                //if (await _authService.RequiresMfaAsync(user))
                //{
                //    return Ok(new
                //    {
                //        success = true,
                //        requiresMfa = true,
                //        message = "MFA authentication required",
                //        tempToken = "GenerateTempToken(user)"
                //    });
                //}
                // Update last login
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Generate Access & Refresh Tokens
                var accessToken = await _tokenService.GenerateJwtToken(user);
                //var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

                Response.Cookies.Append("X-Refresh-Token", accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None, // use Strict only if same-site
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                var response = new AuthResponseDto
                {
                    Token = accessToken,
                    ExpiresIn = Convert.ToInt32(TimeSpan.FromMinutes(30).TotalSeconds),
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = await _userManager.GetRolesAsync(user),
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exception
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "An error occurred while processing your login" });
            }
        }


        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            // SỬ DỤNG PHƯƠNG THỨC GetUserAsync MỚI
            var user = await _authService.GetUserAsync(User);
            if (user == null) return Unauthorized();
           
            var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!passwordValid)
                return BadRequest(new { error = "Current password is incorrect" });

            // Đổi mật khẩu
            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                // Cập nhật last password change sử dụng phương thức mới
                await _authService.UpdateLastPasswordChangeAsync(user);

                // Đăng xuất tất cả các session cũ (tùy chọn)
                await _userManager.UpdateSecurityStampAsync(user);

                return Ok(new
                {
                    message = "Password changed successfully",
                    passwordChangedAt = DateTime.UtcNow
                });
            }

            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        [HttpPost("force-password-change")]
        [Authorize]
        public async Task<IActionResult> ForcePasswordChange([FromBody] ForcePasswordChangeRequest request)
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null) return Unauthorized();

          
            // Reset password mà không cần mật khẩu cũ
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (result.Succeeded)
            {
                
                await _authService.UpdateLastPasswordChangeAsync(user);
                await _userManager.UpdateSecurityStampAsync(user);

                return Ok(new
                {
                    message = "Password changed successfully",
                    passwordChangedAt = user.LastPasswordChangeDate ?? DateTime.UtcNow
                });
            }

            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
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

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest(new { Message = "Invalid user" });

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

            return Ok(new { Message = "Email confirmed successfully" });
        }
        private string GenerateTempToken(IdentityUser user)
        {
            // Tạo token tạm cho MFA flow
            return Guid.NewGuid().ToString();
        }

    }

             
}
