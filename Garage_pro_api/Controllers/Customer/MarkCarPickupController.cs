using BusinessObject.Enums;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Security.Claims;

namespace Garage_pro_api.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarkCarPickupController : ControllerBase
    {
        private readonly IRepairOrderService _repairOrderService;

        public MarkCarPickupController(IRepairOrderService repairOrderService)
        {
            _repairOrderService = repairOrderService;
        }

        [HttpPut("{id:guid}/car-pickup-status")]
        public async Task<IActionResult> UpdateCarPickupStatus(Guid id, [FromBody] UpdateCarPickupStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _repairOrderService.UpdateCarPickupStatusAsync(id, userId, request.CarPickupStatus);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = ex.Message });
            }
        }

    }

    public class UpdateCarPickupStatusRequest
    {
        [Required]
        public CarPickupStatus CarPickupStatus { get; set; }
    }
}
