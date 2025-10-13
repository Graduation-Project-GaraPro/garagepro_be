using BusinessObject;
using Dtos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.ServiceServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _service;

        public ServicesController(IServiceService service)
        {
            _service = service;
        }

        // GET: api/service
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetAll()
        {
            try
            {
                var services = await _service.GetAllServicesAsync();
                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving services", detail = ex.Message });
            }
        }

        [HttpGet("paged")]
        public async Task<ActionResult<object>> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? status = null,
            [FromQuery] Guid? serviceTypeId = null)
        {
            try
            {
                var (services, totalCount) = await _service.GetPagedServicesAsync(
                    pageNumber, pageSize, searchTerm, status, serviceTypeId);

                return Ok(new
                {
                    pageNumber,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    data = services
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving services with paging", detail = ex.Message });
            }
        }


        // GET: api/service/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetById(Guid id)
        {
            try
            {
                var service = await _service.GetServiceByIdAsync(id);
                if (service == null) return NotFound();

                return Ok(service);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving service", detail = ex.Message });
            }
        }

        // POST: api/service
        [HttpPost]
        public async Task<ActionResult<Service>> Create(CreateServiceDto service)
        {
            try
            {
                var created = await _service.CreateServiceAsync(service);
                return CreatedAtAction(nameof(GetById), new { id = created.ServiceId }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating service", detail = ex.Message });
            }
        }

        // PUT: api/service/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<Service>> Update(Guid id, UpdateServiceDto service)
        {
            try
            {
                var updated = await _service.UpdateServiceAsync(id, service);
                if (updated == null) return NotFound();

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating service", detail = ex.Message });
            }
        }
        [HttpPatch("bulk-update-status")]
        public async Task<IActionResult> BulkUpdateServiceStatus([FromBody] BulkUpdateStatusRequest request)
        {
            try
            {
                var result = await _service.BulkUpdateServiceStatusAsync(request.ServiceIds, request.IsActive);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating service status", detail = ex.Message });
            }
        }

        [HttpPatch("bulk-update-advance-status")]
        public async Task<IActionResult> BulkUpdateServiceAdvanceStatus([FromBody] BulkUpdateAdvanceStatusRequest request)
        {
            try
            {
                var result = await _service.BulkUpdateServiceAdvanceStatusAsync(request.ServiceIds, request.IsAdvanced);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating service advance status", detail = ex.Message });
            }
        }
        // DELETE: api/service/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _service.DeleteServiceAsync(id);
                if (!deleted) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting service", detail = ex.Message });
            }
        }
    }
}
