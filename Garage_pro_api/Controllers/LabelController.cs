using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessObject;
using Services;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all endpoints
    public class LabelController : ControllerBase
    {
        private readonly ILabelService _labelService;

        public LabelController(ILabelService labelService)
        {
            _labelService = labelService;
        }

        // GET: api/label
        [HttpGet]
        public async Task<IActionResult> GetAllLabels()
        {
            try
            {
                var labels = await _labelService.GetAllLabelsAsync();
                return Ok(labels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving labels", error = ex.Message });
            }
        }

        // GET: api/label/by-orderstatus/{orderStatusId}
        [HttpGet("by-orderstatus/{orderStatusId}")]
        public async Task<IActionResult> GetLabelsByOrderStatusId(int orderStatusId) // Changed from Guid to int
        {
            try
            {
                var labels = await _labelService.GetLabelsByOrderStatusIdAsync(orderStatusId);
                return Ok(labels);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving labels", error = ex.Message });
            }
        }

        // GET: api/label/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLabelById(Guid id)
        {
            try
            {
                var label = await _labelService.GetLabelByIdAsync(id);
                if (label == null)
                    return NotFound(new { message = $"Label with ID {id} not found" });

                return Ok(label);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the label", error = ex.Message });
            }
        }

        // POST: api/label
        [HttpPost]
        [Authorize(Roles = "Manager")] // Only managers can create labels
        public async Task<IActionResult> CreateLabel([FromBody] CreateLabelRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var label = new Label
                {
                    LabelName = request.LabelName,
                    Description = request.Description ?? string.Empty,
                    ColorName = request.ColorName,
                    HexCode = request.HexCode,
                    OrderStatusId = request.OrderStatusId,
                    IsDefault = request.IsDefault
                };

                var createdLabel = await _labelService.CreateLabelAsync(label);
                return CreatedAtAction(nameof(GetLabelById), 
                    new { id = createdLabel.LabelId }, createdLabel);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the label", error = ex.Message });
            }
        }

        // PUT: api/label/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")] // Only managers can update labels
        public async Task<IActionResult> UpdateLabel(Guid id, [FromBody] UpdateLabelRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var label = new Label
                {
                    LabelId = id,
                    LabelName = request.LabelName,
                    Description = request.Description ?? string.Empty,
                    ColorName = request.ColorName,
                    HexCode = request.HexCode,
                    OrderStatusId = request.OrderStatusId,
                    IsDefault = request.IsDefault
                };

                var updatedLabel = await _labelService.UpdateLabelAsync(label);
                return Ok(updatedLabel);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the label", error = ex.Message });
            }
        }

        // DELETE: api/label/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")] // Only managers can delete labels
        public async Task<IActionResult> DeleteLabel(Guid id)
        {
            try
            {
                var success = await _labelService.DeleteLabelAsync(id);
                if (!success)
                    return NotFound(new { message = $"Label with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the label", error = ex.Message });
            }
        }

        // GET: api/label/{id}/exists
        [HttpGet("{id}/exists")]
        public async Task<IActionResult> CheckLabelExists(Guid id)
        {
            try
            {
                var exists = await _labelService.LabelExistsAsync(id);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking label existence", error = ex.Message });
            }
        }

        // Removed GetAvailableColors endpoint as we're using fixed color data
    }

    // Request DTOs
    public class CreateLabelRequest
    {
        public string LabelName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string HexCode { get; set; } = string.Empty;
        public int OrderStatusId { get; set; }
        public bool IsDefault { get; set; } = false;
    }

    public class UpdateLabelRequest
    {
        public string LabelName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string HexCode { get; set; } = string.Empty;
        public int OrderStatusId { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}