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
        public async Task<ActionResult<IEnumerable<ServiceDto>>> GetServicesByCategoryId(Guid id)
        {
            var services = await _service.GetServicesByCategoryIdAsync(id);
            return Ok(services);
        }
    }
}
