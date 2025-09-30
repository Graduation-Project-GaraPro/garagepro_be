using BusinessObject.Authentication;
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

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            var role = await _roleService.CreateRoleAsync(roleName);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] string newName)
        {
            await _roleService.UpdateRoleAsync(id, newName);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            await _roleService.DeleteRoleAsync(id);
            return NoContent();
        }

        [HttpGet("{id}/permissions")]
        public async Task<IActionResult> GetPermissionsForRole(string id)
            => Ok(await _roleService.GetPermissionsForRoleAsync(id));

        [HttpPost("{id}/permissions/{permissionId}")]
        public async Task<IActionResult> AssignPermission(string id,Guid permissionId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            await _roleService.AssignPermissionAsync(id, permissionId, currentUser.Id);
            return NoContent();
        }

        [HttpDelete("{id}/permissions/{permissionId}")]
        public async Task<IActionResult> RemovePermission(string id, Guid permissionId)
        {
            await _roleService.RemovePermissionAsync(id, permissionId);
            return NoContent();
        }
    }
}
