using Dtos.Parts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PartCategoryServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartCategoriesController : ControllerBase
    {
        private readonly IPartCategoryService _partCategoryService;

        public PartCategoriesController(IPartCategoryService partCategoryService)
        {
            _partCategoryService = partCategoryService;
        }

        // GET: api/partcategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PartCategoryDto>>> GetAll()
        {
            try
            {
                var categories = await _partCategoryService.GetAllPartCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving part categories", detail = ex.Message });
            }
        }

        // GET: api/partcategories/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PartCategoryPagedResultDto>> GetPaged([FromQuery] PaginationDto paginationDto)
        {
            try
            {
                var result = await _partCategoryService.GetPagedPartCategoriesAsync(paginationDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving paged part categories", detail = ex.Message });
            }
        }

        // GET: api/partcategories/search
        [HttpGet("search")]
        public async Task<ActionResult<PartCategoryPagedResultDto>> Search([FromQuery] PartCategorySearchDto searchDto)
        {
            try
            {
                var result = await _partCategoryService.SearchPartCategoriesAsync(searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error searching part categories", detail = ex.Message });
            }
        }

        // GET: api/partcategories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PartCategoryDto>> GetById(Guid id)
        {
            try
            {
                var category = await _partCategoryService.GetPartCategoryByIdAsync(id);
                if (category == null) return NotFound(new { message = "Part category not found" });
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving part category", detail = ex.Message });
            }
        }

        // POST: api/partcategories
        [HttpPost]
        public async Task<ActionResult<PartCategoryDto>> Create([FromBody] CreatePartCategoryDto dto)
        {
            try
            {
                var created = await _partCategoryService.CreatePartCategoryAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.LaborCategoryId }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating part category", detail = ex.Message });
            }
        }

        // PUT: api/partcategories/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<PartCategoryDto>> Update(Guid id, [FromBody] UpdatePartCategoryDto dto)
        {
            try
            {
                var updated = await _partCategoryService.UpdatePartCategoryAsync(id, dto);
                if (updated == null) return NotFound(new { message = "Part category not found" });
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating part category", detail = ex.Message });
            }
        }

        // DELETE: api/partcategories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _partCategoryService.DeletePartCategoryAsync(id);
                if (!deleted) return NotFound(new { message = "Part category not found" });
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting part category", detail = ex.Message });
            }
        }

        // GET: api/partcategories/with-services
        [HttpGet("with-services")]
        public async Task<ActionResult<IEnumerable<PartCategoryWithServicesDto>>> GetAllWithServices()
        {
            try
            {
                var categoriesWithServices = await _partCategoryService.GetAllPartCategoriesWithServicesAsync();
                return Ok(categoriesWithServices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving part categories with services", detail = ex.Message });
            }
        }

        // GET: api/partcategories/{id}/with-services
        [HttpGet("{id}/with-services")]
        public async Task<ActionResult<PartCategoryWithServicesDto>> GetWithServices(Guid id)
        {
            try
            {
                var categoryWithServices = await _partCategoryService.GetPartCategoryWithServicesAsync(id);
                if (categoryWithServices == null) return NotFound(new { message = "Part category not found" });
                return Ok(categoryWithServices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving part category with services", detail = ex.Message });
            }
        }
    }
}