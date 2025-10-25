using AutoMapper;
using Dtos.Auth;
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
        private readonly IMapper _mapper;

        public UsersController(IUserService userService, 
            DynamicAuthenticationService authorizationService,
            IMapper mapper)
        {
            _userService = userService;
            _authorizationService = authorizationService;
            _mapper = mapper;
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
            if (user == null) return Unauthorized();
            var result = await _userService.GetUserByIdAsync(user.Id);
            
            return Ok(_mapper.Map<UserDto>(result));
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserDto model)
        {
            // Lấy user hiện tại từ token (User.Claims)
            var user = await _authorizationService.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "User not found" });

            _mapper.Map(model, user);

            if (await _userService.UpdateUserAsync(user))
            {
                return Ok(_mapper.Map<UserDto>(user));
            }
            else
            {
                return BadRequest(new { message = "Update failed" });
            }
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

        // GET: api/users/managers/without-branch
        //[Authorize(Policy = "USER_VIEW")]
        [HttpGet("managers/without-branch")]
        public async Task<IActionResult> GetManagersWithoutBranch()
        {
            var users = await _userService.GetManagersWithoutBranchAsync();
            return Ok(users.Select(u => new {
                u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                u.Email,
                u.IsActive,
                u.CreatedAt,
                u.LastLogin
            }));
        }

        // GET: api/users/technicians/without-branch
        //[Authorize(Policy = "USER_VIEW")]
        [HttpGet("technicians/without-branch")]
        public async Task<IActionResult> GetTechniciansWithoutBranch()
        {
            var users = await _userService.GetTechniciansWithoutBranchAsync();
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
