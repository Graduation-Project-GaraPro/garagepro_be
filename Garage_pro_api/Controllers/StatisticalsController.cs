using BusinessObject.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.Statistical;

namespace Garage_pro_api.Controllers
{
    [Route("odata/[controller]")]
    [ApiController]
    [Authorize(Roles = "Technician")]
    public class StatisticalsController : ODataController
    {
        private readonly IStatisticalService _service;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatisticalsController(IStatisticalService service, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Bạn cần đăng nhập để xem thống kê." });

            var isTechnician = await _userManager.IsInRoleAsync(user, "Technician");
            if (!isTechnician)
                return Forbid("Chỉ Technician mới có quyền xem thống kê.");

            try
            {
                var result = await _service.GetTechnicianStatisticAsync(user.Id);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
