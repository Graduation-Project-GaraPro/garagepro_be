using Dtos.RepairProgressDto;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RepairProgressServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairProgressController : ControllerBase
    {
        private readonly IRepairProgressService _repairProgressService;

        public RepairProgressController(IRepairProgressService repairProgressService)
        {
            _repairProgressService = repairProgressService;
        }

        [HttpGet("my-orders")]
        public async Task<ActionResult<PagedResult<RepairOrderListItemDto>>> GetMyRepairOrders([FromQuery] RepairOrderFilterDto filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _repairProgressService.GetUserRepairOrdersAsync(userId, filter);
            return Ok(result);
        }

        [HttpGet("{repairOrderId}/progress")]
        public async Task<ActionResult<RepairOrderProgressDto>> GetRepairOrderProgress(Guid repairOrderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var progress = await _repairProgressService.GetRepairOrderProgressAsync(repairOrderId, userId);
            if (progress == null)
            {
                return NotFound("Repair order not found or access denied");
            }

            return Ok(progress);
        }
    }
}
