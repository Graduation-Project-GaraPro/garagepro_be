using Dtos.Branches;
using Dtos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.ServiceServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCategoriesController : ControllerBase
    {
        private readonly IServiceCategoryService _service;

        public ServiceCategoriesController(IServiceCategoryService service)
        {
            _service = service;
        }

        // GET: api/ServiceCategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceCategoryDto>>> GetAll()
        {
            var categories = await _service.GetAllCategoriesAsync();
            return Ok(categories);
        }
        [HttpGet("parents")]
        public async Task<ActionResult<IEnumerable<ServiceCategoryDto>>> GetParentCategories()
        {
            try
            {
                var categories = await _service.GetParentCategoriesAsync();

                if (categories == null || !categories.Any())
                    return NotFound(new { Message = "No parent service categories found." });

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred while retrieving parent categories.",
                    Details = ex.Message
                });
            }
        }

        [HttpGet("fromParent/{parentId}")]
        public async Task<ActionResult<object>> GetAllFromParentCategory(
            Guid parentId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? childServiceCategoryId = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _service.GetAllServiceCategoryFromParentCategoryAsync(
                    parentId,
                    pageNumber,
                    pageSize,
                    childServiceCategoryId,
                    searchTerm
                );

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred while retrieving service categories.",
                    Details = ex.Message
                });
            }
        }

        [HttpGet("forBooking")]
        public async Task<ActionResult<object>> GetAllForBooking(
             [FromQuery] int pageNumber = 1,
             [FromQuery] int pageSize = 10,
             [FromQuery] Guid? serviceCategoryId = null,
             [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (pageNumber <= 0 || pageSize <= 0)
                    return BadRequest(new { Message = "PageNumber and PageSize must be greater than zero." });

                var result = await _service.GetAllCategoriesForBookingAsync(pageNumber, pageSize, serviceCategoryId, searchTerm);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // Lỗi do tham số không hợp lệ
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Lỗi không tìm thấy dữ liệu
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Lỗi hệ thống
                // Gợi ý: có thể log lại lỗi nếu bạn dùng ILogger
                // _logger.LogError(ex, "Error occurred while getting service categories for booking");

                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred while retrieving data.",
                    Details = ex.Message
                });
            }
        }
        // GET: api/ServiceCategories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceCategoryDto>> GetById(Guid id)
        {
            var category = await _service.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        // GET: api/ServiceCategories/{id}/services
        [HttpGet("{id}/services")]
        public async Task<ActionResult<IEnumerable<Dtos.Branches.ServiceDto>>> GetServicesByCategoryId(Guid id)
        {
            var services = await _service.GetServicesByCategoryIdAsync(id);
            return Ok(services);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceCategoryDto>> Create(CreateServiceCategoryDto dto)
        {
            try
            {
                var created = await _service.CreateCategoryAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.ServiceCategoryId }, created);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // log ex nếu cần
                return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceCategoryDto>> Update(Guid id, UpdateServiceCategoryDto dto)
        {
            try
            {
                var updated = await _service.UpdateCategoryAsync(id, dto);
                if (updated == null) return NotFound();

                return Ok(updated);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // log ex nếu cần
                return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteCategoryAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
