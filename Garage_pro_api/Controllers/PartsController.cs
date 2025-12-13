using Dtos.Parts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PartCategoryServices;
using Services;
using System.Security.Claims;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager")]
    public class PartsController : ControllerBase
    {
        private readonly IPartService _partService;
        private readonly IUserService _userService;

        public PartsController(IPartService partService, IUserService userService)
        {
            _partService = partService;
            _userService = userService;
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

        // GET: api/parts/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PartPagedResultDto>> GetPaged([FromQuery] PaginationDto paginationDto)
        {
            try
            {
                var result = await _partService.GetPagedPartsAsync(paginationDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving paged parts", detail = ex.Message });
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

        // GET: api/parts/branch/{branchId}/paged
        [HttpGet("branch/{branchId}/paged")]
        public async Task<ActionResult<PartPagedResultDto>> GetPagedByBranch(Guid branchId, [FromQuery] PaginationDto paginationDto)
        {
            try
            {
                var result = await _partService.GetPagedPartsByBranchAsync(branchId, paginationDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving paged parts by branch", detail = ex.Message });
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
                // Get the authenticated user to extract branch ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // Get the user from the user service to extract their branch ID
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found", userId = userId });
                }

                // Ensure the user has a branch assigned
                if (!user.BranchId.HasValue)
                {
                    return BadRequest(new { message = "Manager must be assigned to a branch to create parts" });
                }

                // Set the branch ID from the logged-in manager
                dto.BranchId = user.BranchId.Value;

                var created = await _partService.CreatePartAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.PartId }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating part", detail = ex.Message });
            }
        }

        // GET: api/parts/{id}/edit
        [HttpGet("{id}/edit")]
        public async Task<ActionResult<EditPartDto>> GetForEdit(Guid id)
        {
            try
            {
                var part = await _partService.GetPartForEditAsync(id);
                if (part == null) return NotFound(new { message = "Part not found" });
                return Ok(part);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving part for edit", detail = ex.Message });
            }
        }

        // PUT: api/parts/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<EditPartDto>> Update(Guid id, [FromBody] UpdatePartDto dto)
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

        // GET: api/parts/service/{serviceId}
        [HttpGet("service/{serviceId}")]
        public async Task<ActionResult<IEnumerable<PartDto>>> GetPartsForService(Guid serviceId)
        {
            try
            {
                var parts = await _partService.GetPartsForServiceAsync(serviceId);
                return Ok(parts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving parts for service", detail = ex.Message });
            }
        }

        // GET: api/parts/service/{serviceId}/categories
        [HttpGet("service/{serviceId}/categories")]
        public async Task<ActionResult<ServicePartCategoryDto>> GetServicePartCategories(Guid serviceId)
        {
            try
            {
                var serviceCategories = await _partService.GetServicePartCategoriesAsync(serviceId);
                if (serviceCategories == null) return NotFound(new { message = "Service not found" });
                return Ok(serviceCategories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving service part categories", detail = ex.Message });
            }
        }

        // PUT: api/parts/service/{serviceId}/categories
        [HttpPut("service/{serviceId}/categories")]
        public async Task<IActionResult> UpdateServicePartCategories(Guid serviceId, [FromBody] List<Guid> partCategoryIds)
        {
            try
            {
                var result = await _partService.UpdateServicePartCategoriesAsync(serviceId, partCategoryIds);
                if (!result) return BadRequest(new { message = "Failed to update service part categories" });
                return Ok(new { message = "Service part categories updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating service part categories", detail = ex.Message });
            }
        }
    }
}
