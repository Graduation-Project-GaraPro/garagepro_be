using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.RoleServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

       
        //[HttpGet]
        //public async Task<IActionResult> GetAllPermissions()
        //{
        //    var permissions = await _permissionService.GetAllPermissionsAsync();
        //    return Ok(permissions);
        //}

        
        [HttpGet("grouped")]
        public async Task<IActionResult> GetAllPermissionsGrouped()
        {
            var permissionsGrouped = await _permissionService.GetAllPermissionsGroupedAsync();
            return Ok(permissionsGrouped);
        }
    }
}
