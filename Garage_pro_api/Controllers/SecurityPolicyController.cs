using BusinessObject.Authentication;
using BusinessObject.Policies;
using Dtos.Policies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.PolicyServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityPolicyController : ControllerBase
    {
        private readonly ISecurityPolicyService _securityPolicyService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SecurityPolicyController(
            ISecurityPolicyService securityPolicyService,
            UserManager<ApplicationUser> userManager)
        {
            _securityPolicyService = securityPolicyService;
            _userManager = userManager;
        }

        // GET: api/admin/securitypolicy/current
        [HttpGet("current")]
        public async Task<ActionResult<SecurityPolicyResponse>> GetCurrentPolicy()
        {
            var policy = await _securityPolicyService.GetCurrentAsync();
            if (policy == null)
                return NotFound("Security policy not found");

            return Ok(MapToResponse(policy));
        }

        // PUT: api/admin/securitypolicy
        [HttpPut]
        public async Task<IActionResult> UpdateSecurityPolicy(UpdateSecurityPolicyRequest request)
        {
            // Lấy AdminId từ người dùng hiện tại
            //var adminUser = await _userManager.GetUserAsync(User);
            //if (adminUser == null)
            //    return Unauthorized();

            //var adminId = adminUser.Id;

            try
            {
                var updatedPolicy = new SecurityPolicy
                {
                    MinPasswordLength = request.MinPasswordLength,
                    RequireSpecialChar = request.RequireSpecialChar,
                    RequireNumber = request.RequireNumber,
                    RequireUppercase = request.RequireUppercase,
                    SessionTimeout = request.SessionTimeout,
                    MaxLoginAttempts = request.MaxLoginAttempts,
                    AccountLockoutTime = request.AccountLockoutTime,
                    MfaRequired = request.MfaRequired,
                    PasswordExpiryDays = request.PasswordExpiryDays,
                    EnableBruteForceProtection = request.EnableBruteForceProtection
                };

                await _securityPolicyService.UpdatePolicyAsync(
                    updatedPolicy,
                    null,
                    request.ChangeSummary
                );

                return Ok(new { message = "Security policy updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/admin/securitypolicy/history
        //[HttpGet("history")]
        //public async Task<ActionResult<IEnumerable<SecurityPolicyHistoryResponse>>> GetPolicyHistory()
        //{
        //    // Giả sử có repository method để lấy history
        //    // var history = await _repo.GetHistoryAsync();
        //    // return Ok(history.Select(MapToHistoryResponse));

        //    return Ok(new { message = "History endpoint to be implemented" });
        //}

        private SecurityPolicyResponse MapToResponse(SecurityPolicy policy)
        {
            return new SecurityPolicyResponse
            {
                MinPasswordLength = policy.MinPasswordLength,
                RequireSpecialChar = policy.RequireSpecialChar,
                RequireNumber = policy.RequireNumber,
                RequireUppercase = policy.RequireUppercase,
                SessionTimeout = policy.SessionTimeout,
                MaxLoginAttempts = policy.MaxLoginAttempts,
                AccountLockoutTime = policy.AccountLockoutTime,
                MfaRequired = policy.MfaRequired,
                PasswordExpiryDays = policy.PasswordExpiryDays,
                EnableBruteForceProtection = policy.EnableBruteForceProtection,
                UpdatedBy = policy.UpdatedBy,
                UpdatedAt = policy.UpdatedAt
            };
        }
    }
}