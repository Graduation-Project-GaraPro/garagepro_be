using BusinessObject.Campaigns;
using Dtos.Campaigns;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.CampaignServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionalCampaignsController : ControllerBase
    {
        private readonly IPromotionalCampaignService _service;

        public PromotionalCampaignsController(IPromotionalCampaignService service)
        {
            _service = service;
        }
        [HttpGet("paged")]
        public async Task<ActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? search = null,
            [FromQuery] CampaignType? type = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            if (page <= 0) page = 1;
            if (limit <= 0) limit = 10;

            var (campaigns, totalCount) = await _service.GetPagedAsync(page, limit, search, type, isActive, startDate, endDate);

            return Ok(new
            {
                Data = campaigns,
                Pagination = new
                {
                    Page = page,
                    Limit = limit,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)limit)
                }
            });
        }
        // GET: api/promotionalcampaigns
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromotionalCampaignDto>>> GetAll()
        {
            try
            {
                var campaigns = await _service.GetAllAsync();
                return Ok(campaigns);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving campaigns", detail = ex.Message });
            }
        }

        // GET: api/promotionalcampaigns/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PromotionalCampaignDto>> GetById(Guid id)
        {
            try
            {
                var campaign = await _service.GetByIdAsync(id);
                if (campaign == null) return NotFound(new { message = "Campaign not found" });

                return Ok(campaign);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving campaign", detail = ex.Message });
            }
        }

        // POST: api/promotionalcampaigns
        [HttpPost]
        public async Task<ActionResult<PromotionalCampaignDto>> Create([FromBody] CreatePromotionalCampaignDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating campaign", detail = ex.Message });
            }
        }

        // PUT: api/promotionalcampaigns/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<PromotionalCampaignDto>> Update(Guid id, [FromBody] UpdatePromotionalCampaignDto dto)
        {
            try
            {
                if (id != dto.Id) return BadRequest(new { message = "Id mismatch" });

                var updated = await _service.UpdateAsync(dto);
                if (updated == null) return NotFound(new { message = "Campaign not found" });

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating campaign", detail = ex.Message });
            }
        }
        [HttpPost("bulk/activate")]
        public async Task<IActionResult> BulkActivate([FromBody] List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest(new { message = "No campaign ids provided." });

            var result = await _service.BulkUpdateStatusAsync(ids, true);

            if (!result)
                return NotFound(new { message = "No campaigns found to activate." });

            return Ok(new { message = "Campaigns activated successfully." });
        }

        [HttpPost("bulk/deactivate")]
        public async Task<IActionResult> BulkDeactivate([FromBody] List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest(new { message = "No campaign ids provided." });

            var result = await _service.BulkUpdateStatusAsync(ids, false);

            if (!result)
                return NotFound(new { message = "No campaigns found to deactivate." });

            return Ok(new { message = "Campaigns deactivated successfully." });
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            var result = await _service.ActivateAsync(id);
            if (!result)
                return NotFound(new { message = "Campaign not found." });

            return Ok(new { message = "Campaign activated successfully." });
        }

        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var result = await _service.DeactivateAsync(id);
            if (!result)
                return NotFound(new { message = "Campaign not found." });

            return Ok(new { message = "Campaign deactivated successfully." });
        }


        [HttpDelete("range")]
        public async Task<IActionResult> DeleteRange([FromBody] List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest(new { message = "No campaign ids provided." });

            try
            {
                var result = await _service.DeleteRangeAsync(ids);

                if (!result)
                    return NotFound(new { message = "No campaigns found to delete." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // DELETE: api/promotionalcampaigns/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _service.DeleteAsync(id);
                if (!deleted) return NotFound(new { message = "Campaign not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting campaign", detail = ex.Message });
            }
        }
    }
}
