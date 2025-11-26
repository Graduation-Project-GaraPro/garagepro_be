using Dtos.RepairProgressDto;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RepairProgressServices;
using BusinessObject;
using Dtos.RepairOrderArchivedDtos;
using Microsoft.AspNetCore.Authorization;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
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

        [HttpGet("archived")]
        public async Task<ActionResult<PagedResult<RepairOrderArchivedListItemDto>>> GetArchived(
        [FromQuery] RepairOrderFilterDto filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _repairProgressService.GetArchivedRepairOrdersAsync(userId, filter);
            return Ok(result);
        }

        [HttpGet("archived/{repairOrderId}")]
        public async Task<ActionResult<RepairOrderArchivedDetailDto>> GetArchivedDetail(Guid repairOrderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var dto = await _repairProgressService.GetArchivedRepairOrderDetailAsync(repairOrderId, userId);
            if (dto == null) return NotFound();
            return Ok(dto);
        }
    }
}
