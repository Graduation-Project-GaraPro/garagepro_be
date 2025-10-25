using BusinessObject.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.RepairHistory;

namespace Garage_pro_api.Controllers
{
    [Route("odata/[controller]")]
    [ApiController]
    [Authorize(Roles = "Technician")]
    public class RepairHistoriesController : ODataController
    {
        private readonly IRepairHistoryService _service;
        private readonly UserManager<ApplicationUser> _userManager;

        public RepairHistoriesController(IRepairHistoryService service, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        [HttpGet("my-historys")]
        public async Task<IActionResult> GetMyHistorys()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Bạn cần đăng nhập để xem lịch sử sửa chữa." });

            var isTechnician = await _userManager.IsInRoleAsync(user, "Technician");
            if (!isTechnician)
                return Forbid("Chỉ Technician mới có quyền xem lịch sử sửa chữa.");

            try
            {
                var result = await _service.GetRepairHistoryByUserIdAsync(user.Id);
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
