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

        // GET: api/promotionalcampaigns
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromotionalCampaignDto>>> GetAll()
        {
            var campaigns = await _service.GetAllAsync();
            return Ok(campaigns);
        }

        // GET: api/promotionalcampaigns/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PromotionalCampaignDto>> GetById(Guid id)
        {
            var campaign = await _service.GetByIdAsync(id);
            if (campaign == null) return NotFound();
            return Ok(campaign);
        }

        // POST: api/promotionalcampaigns
        [HttpPost]
        public async Task<ActionResult<PromotionalCampaignDto>> Create([FromBody] CreatePromotionalCampaignDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/promotionalcampaigns/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<PromotionalCampaignDto>> Update(Guid id, [FromBody] UpdatePromotionalCampaignDto dto)
        {
            if (id != dto.Id) return BadRequest("Id mismatch");

            var updated = await _service.UpdateAsync(dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        // DELETE: api/promotionalcampaigns/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
