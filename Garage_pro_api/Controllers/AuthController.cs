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
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Services.SmsSenders;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Services.LogServices;
using Services.PolicyServices;

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
        private readonly IConfiguration _config;
        private readonly ISecurityPolicyService _securityPolicyService;
        private readonly ISmsSender _smsSender;
        private readonly ILogService _logService;
        // Lưu OTP tạm thời (dev/test)
        private static readonly Dictionary<string, string> _otpTempStore = new();
        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService, IEmailSender emailSender, ISecurityPolicyService iSecurityPolicyService, DynamicAuthenticationService dynamicAuthenticationService, IConfiguration config, ISmsSender smsSender, ILogService logService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailSender = emailSender;
            _authService = dynamicAuthenticationService;
            _config = config;
            _smsSender = smsSender;
            _logService = logService;
            _securityPolicyService = iSecurityPolicyService;
        }

        //  Gửi OTP
        [HttpPost("send-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Kiểm tra số điện thoại đã tồn tại chưa
            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

            if (existingUser != null)
            {
                // Nếu đã có thì trả lỗi theo format model validation

                return BadRequest(new { error = "Phone number already existing!" });
            }

            // Tạo OTP 6 số
            var otp = new Random().Next(100000, 999999).ToString();

            // Lưu OTP tạm
            _otpTempStore[model.PhoneNumber] = otp;

            // Gửi SMS (Fake)
            await _smsSender.SendSmsAsync(model.PhoneNumber, otp);

            return Ok(new { Message = "OTP sent successfully (dev/fake)" });
        }
        // 2Xác thực OTP
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public IActionResult VerifyOtp([FromBody] VerifyOtpDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var valid = FakeSmsSender.VerifyOtp(model.PhoneNumber, model.Token);
            if (!valid) return BadRequest(new { error = "Invalid or expired OTP" });

            return Ok(new { Message = "OTP verified successfully" });
        }
        // Hoàn tất đăng ký
        [HttpPost("complete-registration")]
        [AllowAnonymous]
        public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!FakeSmsSender.IsVerified(model.PhoneNumber))
                return BadRequest(new { error = "Phone number not verified" });

            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
            if (existingUser != null)
                return BadRequest(new { error = "Phone number already registered" });

            if (!string.IsNullOrEmpty(model.Email))
            {
                var existingEmailUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingEmailUser != null)
                    return BadRequest(new { error = "Email already in use" });
            }

            var user = new ApplicationUser
            {
                UserName = model.PhoneNumber,
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.UtcNow,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

            await _userManager.AddToRoleAsync(user, "Customer");



            return Ok(new
            {
                Message = "User registered successfully",
                UserId = user.Id
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CompleteRegistrationDto model)
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
            {
                var errorDict = new Dictionary<string, string[]>();

                // gom tất cả lỗi IdentityError lại
                var passwordErrors = result.Errors.Select(e => e.Description).ToArray();

                if (passwordErrors.Any())
                {
                    errorDict["Password"] = passwordErrors;
                }

                return BadRequest(new { Errors = errorDict });
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "Customer");

            // Send confirmation email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);

            var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

            Console.WriteLine($"Confirmation Link: {confirmationLink}"); // For debugging

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


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var signInResult = await _authService.PasswordSignInAsync(model.PhoneNumber, model.Password);

            if (signInResult == Microsoft.AspNetCore.Identity.SignInResult.LockedOut)
            {
                var userInfor = await _userManager.Users
                    .FirstAsync(u => u.PhoneNumber == model.PhoneNumber);

                var lockoutInfo = await _authService.GetLockoutInfoAsync(userInfor);

                var message = $"User '{userInfor.UserName}' (Email: {userInfor.Email}, Phone: {userInfor.PhoneNumber}) " +
                              $"has been locked out due to multiple failed login attempts. " +
                              $"Lockout end time: {lockoutInfo.LockoutEnd?.ToLocalTime():f}.";

                await _logService.LogSecurityAsync(message, userInfor.Id);

                return BadRequest(new
                {
                    error = "Account locked out",
                    details = lockoutInfo
                });
            }

            if (signInResult == Microsoft.AspNetCore.Identity.SignInResult.Failed)
                return BadRequest(new { error = "Invalid phone number or password." });

            if (signInResult == Microsoft.AspNetCore.Identity.SignInResult.NotAllowed)
                return BadRequest(new { error = "User is not allowed to sign in." });

            //// Tạo JWT token
            //var accessToken = await _tokenService.GenerateJwtToken(user, 30); 
            //var refreshToken = await _tokenService.GenerateJwtToken(user, 7 * 24 * 60); 

            // Đăng nhập thành công
            var user = await _userManager.Users.FirstAsync(u => u.PhoneNumber == model.PhoneNumber);

            // Lấy policy hiện tại
            var policy = await _securityPolicyService.GetCurrentAsync();
            if (policy == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Security policy not found." });

            // Phát hành token theo policy
            var accessToken = await _tokenService.GenerateAccessToken(user);

            // Refresh token: ví dụ 7 ngày (tuỳ bạn)
            var refreshLifetimeMinutes = 7 * 24 * 60;
            var refreshToken = await _tokenService.GenerateRefreshToken(user, refreshLifetimeMinutes);

            // Cookie refresh (tùy chọn)
            Response.Cookies.Append("X-Refresh-Token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(refreshLifetimeMinutes)
            });

            var response = new AuthResponseDto
            {
                Token = accessToken,
                ExpiresIn = policy.SessionTimeout * 60, // giây, khớp với access token
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Roles = await _userManager.GetRolesAsync(user),
            };

            return Ok(response);
        }


        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            var payload = await VerifyGoogleToken(dto);
            if (payload == null)
            {
                return BadRequest("Invalid Google token");
            }

            // Kiểm tra người dùng có tồn tại chưa
            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    EmailConfirmed = true // Google đã xác minh email
                };
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                // Gán role mặc định
                await _userManager.AddToRoleAsync(user, "Customer");
            }

            // Get user roles to determine token expiration time
            var userRoles = await _userManager.GetRolesAsync(user);
            var jwtSettings = _config.GetSection("Jwt");
            int accessTokenExpiryMinutes = jwtSettings.GetValue<int>("ExpireMinutes", 30); // Default expiry time

            // Check if user has manager role and set longer expiration time
            if (userRoles.Contains("Manager") || userRoles.Contains("Admin"))
            {
                accessTokenExpiryMinutes = jwtSettings.GetValue<int>("ManagerExpireMinutes", 120); // Manager/Admin expiry time
            }

            var accessToken = await _tokenService.GenerateAccessToken(user);

            var response = new AuthResponseDto
            {
                Token = accessToken,
                ExpiresIn = Convert.ToInt32(TimeSpan.FromMinutes(accessTokenExpiryMinutes).TotalSeconds),
                UserId = user.Id,
                Email = user.Email,
                Roles = await _userManager.GetRolesAsync(user),
            };

            return Ok(response);
        }
        //https://developers.google.com/oauthplayground
        private async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleToken(GoogleLoginDto dto)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { _config["Authentication:Google:ClientId"] }
                };
                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
                return payload;
            }
            catch
            {
                return null;
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

                var refreshLifetimeMinutes = 7 * 24 * 60;
                var newToken = await _tokenService.GenerateRefreshToken(user, refreshLifetimeMinutes);


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
        // 3 Dev helper: lấy OTP trực tiếp (chỉ dev)
        [HttpGet("dev/last-otp")]
        public IActionResult GetLastOtp([FromQuery] string phoneNumber)
        {
            if (FakeSmsSender.TryGetLastOtp(phoneNumber, out var otp))
                return Ok(new { phoneNumber, otp });

            return NotFound(new { error = "No OTP found for this number" });
        }

        [HttpPost("forgot-password/send-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordSendOtp([FromBody] SendOtpDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

            if (user == null)
                return BadRequest(new { error = "Phone number not found" });

            // Tạo OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Lưu OTP tạm
            _otpTempStore[model.PhoneNumber] = otp;

            await _smsSender.SendSmsAsync(model.PhoneNumber, otp);

            return Ok(new { message = "OTP sent for password reset" });
        }

        [HttpPost("forgot-password/verify-otp")]
        [AllowAnonymous]
        public IActionResult ForgotPasswordVerifyOtp([FromBody] VerifyOtpDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check OTP
            if (!_otpTempStore.TryGetValue(model.PhoneNumber, out var otp))
                return BadRequest(new { error = "OTP not found" });

            if (otp != model.Token)
                return BadRequest(new { error = "Invalid OTP" });

            // OTP đúng → tạo reset token (GUID)
            var resetToken = Guid.NewGuid().ToString("N");

            // Lưu token tạm
            _otpTempStore[$"reset:{model.PhoneNumber}"] = resetToken;

            return Ok(new
            {
                message = "OTP verified",
                resetToken
            });
        }

        [HttpPost("forgot-password/reset")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordReset([FromBody] PasswordChangeRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra reset token
            if (!_otpTempStore.TryGetValue($"reset:{model.PhoneNumber}", out var savedToken))
                return BadRequest(new { error = "Reset token not found" });

            if (savedToken != model.ResetToken)
                return BadRequest(new { error = "Invalid reset token" });

            // Lấy user
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

            if (user == null)
                return BadRequest(new { error = "User not found" });

            // Reset password (không cần mật khẩu cũ)
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            // Xoá token sau khi dùng
            _otpTempStore.Remove($"reset:{model.PhoneNumber}");

            // Cập nhật security stamp
            await _userManager.UpdateSecurityStampAsync(user);

            return Ok(new { message = "Password reset successfully" });
        }

    }


}