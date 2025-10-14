using Dtos.Parts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.PartCategoryServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartCategoriesController : ControllerBase
    {
        private readonly IPartCategoryService _service;

        public PartCategoriesController(IPartCategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PartCategoryWithPartsDto>>> GetAll()
        {
            var result = await _service.GetAllWithPartsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PartCategoryWithPartsDto>> GetById(Guid id)
        {
            var result = await _service.GetByIdWithPartsAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
