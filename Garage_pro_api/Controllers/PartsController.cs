using Dtos.Parts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PartCategoryServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager")]
    public class PartsController : ControllerBase
    {
        private readonly IPartService _partService;

        public PartsController(IPartService partService)
        {
            _partService = partService;
        }

        // GET: api/parts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PartDto>>> GetAll()
        {
            try
            {
                var parts = await _partService.GetAllPartsAsync();
                return Ok(parts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving parts", detail = ex.Message });
            }
        }

        // GET: api/parts/search
        [HttpGet("search")]
        public async Task<ActionResult<PartPagedResultDto>> Search([FromQuery] PartSearchDto searchDto)
        {
            try
            {
                var result = await _partService.SearchPartsAsync(searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error searching parts", detail = ex.Message });
            }
        }

        // GET: api/parts/branch/{branchId}
        [HttpGet("branch/{branchId}")]
        public async Task<ActionResult<IEnumerable<PartDto>>> GetByBranch(Guid branchId)
        {
            try
            {
                var parts = await _partService.GetPartsByBranchAsync(branchId);
                return Ok(parts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving parts", detail = ex.Message });
            }
        }

        // GET: api/parts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PartDto>> GetById(Guid id)
        {
            try
            {
                var part = await _partService.GetPartByIdAsync(id);
                if (part == null) return NotFound(new { message = "Part not found" });
                return Ok(part);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving part", detail = ex.Message });
            }
        }

        // POST: api/parts
        [HttpPost]
        public async Task<ActionResult<PartDto>> Create([FromBody] CreatePartDto dto)
        {
            try
            {
                var created = await _partService.CreatePartAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.PartId }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating part", detail = ex.Message });
            }
        }

        // PUT: api/parts/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<PartDto>> Update(Guid id, [FromBody] UpdatePartDto dto)
        {
            try
            {
                var updated = await _partService.UpdatePartAsync(id, dto);
                if (updated == null) return NotFound(new { message = "Part not found" });
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating part", detail = ex.Message });
            }
        }

        // DELETE: api/parts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _partService.DeletePartAsync(id);
                if (!deleted) return NotFound(new { message = "Part not found" });
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting part", detail = ex.Message });
            }
        }
    }
}
