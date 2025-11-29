using System.Data;
using BusinessObject.Authentication;
using CloudinaryDotNet.Actions;
using Dtos.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.RoleServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController(IRoleService roleService, UserManager<ApplicationUser> userManager)
        {
            _roleService = roleService;
            _userManager = userManager;
        }
        [Authorize(Policy = "ROLE_VIEW")]
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
            => Ok(await _roleService.GetAllRolesAsync());


        [Authorize(Policy = "ROLE_VIEW")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpPost("{roleId}/assign-users")]
        public async Task<IActionResult> AssignRoleToUsers(string roleId, [FromBody] AssignRoleToUsersRequest request)
        {

            try
            {
                var dto = new AssignRoleToUsersDto
                {
                    RoleId = roleId,
                    UserIds = request.UserIds,
                    GrantedBy = User?.Identity?.Name ?? "system"
                };

                //await _roleService.AssignRoleToUsersAsync(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest( new { message = ex.Message });
            }
        }
        [Authorize(Policy = "ROLE_CREATE")]

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto request)
        {
            try
            {
                var role = await _roleService.CreateRoleAsync(request);

                return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
            }
            catch (ArgumentException ex) // lỗi do input không hợp lệ
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) // lỗi nghiệp vụ
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex) // lỗi hệ thống
            {
                // TODO: log exception
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message});
            }
        }
        [Authorize(Policy = "ROLE_UPDATE")]

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleDto request)
        {
            try
            {
                var updatedRole = await _roleService.UpdateRoleAsync(request);

                if (updatedRole == null)
                    return NotFound(new { message = $"Role with id {id} not found." });

                return Ok(updatedRole);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // TODO: log exception
                return StatusCode(StatusCodes.Status500InternalServerError, new { message =  ex.Message });
            }
        }
        [Authorize(Policy = "ROLE_DELETE")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                    throw new KeyNotFoundException("Role not found");
                // Check if role is default
                if (role.IsDefault)
                    throw new InvalidOperationException("Cannot delete default role.");


                var users = await _roleService.GetUsersByRoleIdAsync(role.Id);
                if (users.Count > 0)
                    throw new InvalidOperationException("Cannot delete role. This role is currently assigned to users.");

                await _roleService.DeleteRoleAsync(id);
                return NoContent();
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = ex.Message });
            }
           
        }
        [Authorize(Policy = "ROLE_VIEW")]
        [HttpGet("{roleId}/users")]
        public async Task<IActionResult> GetUsersByRole(string roleId)
        {
            try
            {
                var users = await _roleService.GetUsersByRoleIdAsync(roleId);

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = ex.Message });
            }
        }


    }
    public class AssignRoleToUsersRequest
    {
        public List<string> UserIds { get; set; } = new();
    }
}
