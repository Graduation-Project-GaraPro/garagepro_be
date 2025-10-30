using System.Security.Claims;
using System.Text.Json;
using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Policies;
using Dtos.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.PolicyServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [Authorize("POLICY_MANAGEMENT")]
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


        // PUT: api/securitypolicies/{policyId}/revert-to-snapshot/{historyId}
        [HttpPut("revert-to-snapshot/{historyId}")]
        public async Task<IActionResult> RevertToSnapshot(Guid historyId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                var policy = await _securityPolicyService.RevertToSnapshotAsync(historyId,userId);
                return Ok(policy);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/securitypolicies/revert-to-previous/{historyId}
        [HttpPut("revert-to-previous/{historyId}")]
        public async Task<IActionResult> RevertToPrevious(Guid historyId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                var policy = await _securityPolicyService.UndoChangeAsync(historyId, userId);
                return Ok(policy);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
                    //MfaRequired = request.MfaRequired,
                    PasswordExpiryDays = request.PasswordExpiryDays,
                    EnableBruteForceProtection = request.EnableBruteForceProtection,
                    UpdatedBy = userId,
                };

                await _securityPolicyService.UpdatePolicyAsync(
                    updatedPolicy,
                    userId,
                    request.ChangeSummary
                );

                return Ok(new { message = "Security policy updated successfully" , updatedPolicy});
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


        [HttpGet("history")]
        public async Task<IActionResult> GetAllHistory([FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? changedBy = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            var response = await _securityPolicyService.GetAuditHistoryAsync(page, pageSize, search, changedBy, dateFrom, dateTo);
            return Ok(response);
        }
        [HttpGet("{historyId}/history")]
        public async Task<IActionResult> GetHistory(Guid historyId)
        {
            var history = await _securityPolicyService.GetHistoryAsync(historyId);
            if (history == null)
                return NotFound("No history found for this policy.");

            return Ok(history);
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
                
                PasswordExpiryDays = policy.PasswordExpiryDays,
                EnableBruteForceProtection = policy.EnableBruteForceProtection,
                UpdatedBy = policy.UpdatedBy,
                UpdatedAt = policy.UpdatedAt
            };
        }
    }
}