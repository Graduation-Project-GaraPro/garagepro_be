using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
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

        //[Authorize(Policy = "USER_VIEW")]
        [HttpGet("managers-technicians")]
        public async Task<IActionResult> GetManagersAndTechnicians()
        {
            var users = await _userService.GetManagersAndTechniciansAsync();
            return Ok(users.Select(u => new {
                u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                u.Email,
                u.IsActive,
                u.CreatedAt,
                u.LastLogin
            }));
        }

        //[Authorize(Policy = "USER_VIEW")]
        [HttpGet("managers")]
        public async Task<IActionResult> GetManagers()
        {
            var users = await _userService.GetManagersAsync();
            return Ok(users.Select(u => new {
                u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                u.Email,
                u.IsActive,
                u.CreatedAt,
                u.LastLogin
            }));
        }

        //[Authorize(Policy = "USER_VIEW")]
        [HttpGet("technicians")]
        public async Task<IActionResult> GetTechnicians()
        {
            var users = await _userService.GetTechniciansAsync();
            return Ok(users.Select(u => new {
                u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                u.Email,
                u.IsActive,
                u.CreatedAt,
                u.LastLogin
            }));
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
