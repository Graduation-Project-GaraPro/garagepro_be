using BusinessObject.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedBackController : ControllerBase
    {
        private readonly IFeedBackService _feedbackService;
        public FeedBackController(IFeedBackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var feedbacks = await _feedbackService.GetAllAsync();
            return Ok(feedbacks);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FeedBack feedback)
        {
            if (feedback == null)
            {
                return BadRequest("Feedback is null.");
            }
            await _feedbackService.AddAsync(feedback);
            return CreatedAtAction(nameof(feedback), new { id = feedback.FeedBackId }, feedback);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] FeedBack feedback)
        {
            if (feedback == null || id != feedback.FeedBackId)
            {
                return BadRequest("Feedback is null or ID mismatch.");
            }
            var existingFeedback = await _feedbackService.GetByIdAsync(id);
            if (existingFeedback == null)
            {
                return NotFound("Feedback not found.");
            }
            await _feedbackService.UpdateAsync(feedback);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingFeedback = await _feedbackService.GetByIdAsync(id);
            if (existingFeedback == null)
            {
                return NotFound("Feedback not found.");
            }
            await _feedbackService.DeleteAsync(id);
            return NoContent();
        }
    }
}
