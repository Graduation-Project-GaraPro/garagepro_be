using BusinessObject.Authentication;
using Dtos.Roles;
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

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
            => Ok(await _roleService.GetAllRolesAsync());

        

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            await _roleService.DeleteRoleAsync(id);
            return NoContent();
        }

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


        //[HttpGet("{id}/permissions")]
        //public async Task<IActionResult> GetPermissionsForRole(string id)
        //    => Ok(await _roleService.GetPermissionsForRoleAsync(id));

        //[HttpPost("{id}/permissions/{permissionId}")]
        //public async Task<IActionResult> AssignPermission(string id,Guid permissionId)
        //{
        //    var currentUser = await _userManager.GetUserAsync(User);
        //    await _roleService.AssignPermissionAsync(id, permissionId, currentUser.Id);
        //    return NoContent();
        //}

        //[HttpDelete("{id}/permissions/{permissionId}")]
        //public async Task<IActionResult> RemovePermission(string id, Guid permissionId)
        //{
        //    await _roleService.RemovePermissionAsync(id, permissionId);
        //    return NoContent();
        //}
    }
}
