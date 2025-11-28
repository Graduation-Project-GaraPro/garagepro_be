using Dtos.Parts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PartCategoryServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        // GET: api/parts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PartDto>> GetById(Guid id)
        {
            try
            {
                var part = await _partService.GetPartByIdAsync(id);
                if (part == null) return NotFound();
                return Ok(part);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving part", detail = ex.Message });
            }
        }

        // GET: api/parts/service/{serviceId}
        //[HttpGet("service/{serviceId}")]
        //public async Task<ActionResult<IEnumerable<PartByServiceDto>>> GetPartsByServiceId(Guid serviceId)
        //{
        //    try
        //    {
        //        var parts = await _partService.GetPartsByServiceIdAsync(serviceId);
        //        return Ok(parts);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Error retrieving parts by service ID", detail = ex.Message });
        //    }
        //}

        // POST: api/parts
        [HttpPost]
        public async Task<ActionResult<PartDto>> Create(CreatePartDto part)
        {
            try
            {
                var created = await _partService.CreatePartAsync(part);
                return CreatedAtAction(nameof(GetById), new { id = created.PartId }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating part", detail = ex.Message });
            }
        }

        // PUT: api/parts/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<PartDto>> Update(Guid id, UpdatePartDto part)
        {
            try
            {
                var updated = await _partService.UpdatePartAsync(id, part);
                if (updated == null) return NotFound();

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
                if (!deleted) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting part", detail = ex.Message });
            }
        }
    }
}