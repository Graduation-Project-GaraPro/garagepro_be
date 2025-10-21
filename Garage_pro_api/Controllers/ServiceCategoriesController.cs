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
        [HttpGet("forBooking")]
        public async Task<ActionResult<IEnumerable<ServiceCategoryForBooking>>> GetAllBasic()
        {
            var categories = await _service.GetAllCategoriesForBookingAsync();
            return Ok(categories);
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
