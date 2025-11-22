using AutoMapper;
using BusinessObject;
using BusinessObject.Roles;
using Dtos.Auth;
using Dtos.Customers;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Authentication;
using Services.RoleServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly DynamicAuthenticationService _authorizationService;
        private readonly IPermissionService _permissionService;
        private readonly IMapper _mapper;

        public UsersController(IUserService userService, 
            DynamicAuthenticationService authorizationService, IPermissionService permissionService, RoleManager<ApplicationRole> roleManager,
            IMapper mapper)
        {
            _userService = userService;
            _authorizationService = authorizationService;
            _permissionService = permissionService;
            _roleManager = roleManager;
            _mapper = mapper;
        }
        [Authorize(Policy = "USER_VIEW")]
        // GET: api/users
        [HttpGet]

        public async Task<IActionResult> GetUsers([FromQuery] UserFilterDto filters)
        {
            var (data, total) = await _userService.GetUsersFiltered(filters);
            return Ok(new { total, filters.Page, filters.Limit, data });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            var result = await _userService.CreateUserAsync(dto);
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

        [HttpGet("permissions")]
        public async Task<IActionResult> GetMyPermissions()
        {
            var user = await _authorizationService.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var roleNames = await _userService.GetUserRolesAsync(user);

            var permissionCodes = new HashSet<string>();

            foreach (var roleName in roleNames)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var rolePermissionCodes = await _permissionService.GetPermissionsByRoleIdAsync(role.Id);
                foreach (var code in rolePermissionCodes)
                    permissionCodes.Add(code);
            }

            return Ok(new
            {
                permissions = permissionCodes.ToList(),
                // Có thể trả kèm version/timestamp nếu muốn
                fetchedAt = DateTime.UtcNow
            });
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
        [Authorize]
        [HttpPut("device")]
        public async Task<IActionResult> UpdateDeviceId([FromBody] UpdateDeviceIdRequest request)
        {

            var user = await _authorizationService.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "User not found" });

            var success = await _userService.UpdateDeviceIdAsync(user.Id, request.DeviceId);

            if (!success)
                return NotFound(new { message = "User not found or update failed." });

            return Ok(new { message = "DeviceId updated successfully." });
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
        
        // GET: api/users/technicians/by-branch/{branchId}
        //[Authorize(Policy = "USER_VIEW")]
        [HttpGet("technicians/by-branch/{branchId}")]
        public async Task<IActionResult> GetTechniciansByBranch(Guid branchId)
        {
            var users = await _userService.GetTechniciansByBranchAsync(branchId);
            return Ok(users.Select(u => new {
                u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                u.Email,
                u.IsActive,
                u.CreatedAt,
                u.LastLogin,
                u.BranchId
            }));
        }

        // PUT: api/users/{id}/ban
        [HttpPut("{id}/ban")]
        public async Task<IActionResult> BanUser(string id, [FromBody] BanUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "Ban reason is required" });

            var success = await _userService.BanUserAsync(id, request.Message);
            if (!success)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "User banned successfully" });
        }


        // PUT: api/users/{id}/unban
        [HttpPut("{id}/unban")]
        public async Task<IActionResult> UnbanUser(string id)
        {
            var message = "User unbanned by admin";
            var success = await _userService.UnbanUserAsync(id, message);
            if (!success) return NotFound(new { message = "User not found" });

            return Ok(new { message = "User unbanned successfully" });
        }

        [HttpPut("{id}/verify")]
        public async Task<IActionResult> VerifyUser(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });
            user.EmailConfirmed = true;
            var updated = await _userService.UpdateUserAsync(user);
            if (!updated)
                return BadRequest(new { message = "Failed to verify email" });
            return Ok(new { message = "Email verified successfully" });
        }
        
    }
    public class UpdateDeviceIdRequest
    {
        public string DeviceId { get; set; } = string.Empty;
    }
}
