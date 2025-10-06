using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Authentication;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly DynamicAuthenticationService _authorizationService;

        public UsersController(IUserService userService, DynamicAuthenticationService authorizationService)
        {
            _userService = userService;
            _authorizationService = authorizationService;
        }
        [Authorize(Policy = "USER_VIEW")]
        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            var result = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userService.GetUserRolesAsync(user);
                result.Add(new
                {
                    user.Id,
                    FullName = $"{user.FirstName} {user.LastName}",
                    user.Email,
                    user.IsActive,
                    user.CreatedAt,
                    user.EmailConfirmed,
                    user.LastLogin,
                    Roles = roles
                });
            }

            return Ok(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            
            var user = await _authorizationService.GetUserAsync(User);
            if (user == null) return NotFound(new { message = "User not found" });
            var roles = await _userService.GetUserRolesAsync(user);
            var result = new
            {
                user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                user.Email,
                user.IsActive,
                user.CreatedAt,
                user.EmailConfirmed,
                user.LastLogin,
                Roles = roles
            };
            return Ok(result);
        }

        // PUT: api/users/{id}/ban
        [HttpPut("{id}/ban")]
        public async Task<IActionResult> BanUser(string id ,string message)
        {
            var success = await _userService.BanUserAsync(id, message);
            if (!success) return NotFound(new { message = "User not found" });

            return Ok(new { message = "User banned successfully" });
        }

        // PUT: api/users/{id}/unban
        [HttpPut("{id}/unban")]
        public async Task<IActionResult> UnbanUser(string id, string message)
        {
            var success = await _userService.UnbanUserAsync(id, message);
            if (!success) return NotFound(new { message = "User not found" });

            return Ok(new { message = "User unbanned successfully" });
        }
    }
}
