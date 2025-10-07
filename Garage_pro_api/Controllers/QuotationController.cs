using Microsoft.AspNetCore.Mvc;
using Services;
using Dtos.Quotation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuotationController : ControllerBase
    {
        private readonly IQuotationService _quotationService;

        public QuotationController(IQuotationService quotationService)
        {
            _quotationService = quotationService;
        }

        // GET: api/Quotation
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuotationDto>>> GetQuotations()
        {
            var quotations = await _quotationService.GetAllQuotationsAsync();
            return Ok(quotations);
        }

        // GET: api/Quotation/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuotationDto>> GetQuotation(Guid id)
        {
            var quotation = await _quotationService.GetQuotationByIdAsync(id);
            if (quotation == null)
            {
                return NotFound();
            }
            return Ok(quotation);
        }

        // GET: api/Quotation/inspection/5
        [HttpGet("inspection/{inspectionId}")]
        public async Task<ActionResult<QuotationDto>> GetQuotationByInspectionId(Guid inspectionId)
        {
            var quotation = await _quotationService.GetQuotationByInspectionIdAsync(inspectionId);
            if (quotation == null)
            {
                return NotFound();
            }
            return Ok(quotation);
        }

        // POST: api/Quotation
        [HttpPost]
        public async Task<ActionResult<QuotationDto>> CreateQuotation(Guid inspectionId)
        {
            // Get user ID from claims
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var quotation = await _quotationService.CreateQuotationAsync(inspectionId, userId);
            return CreatedAtAction(nameof(GetQuotation), new { id = quotation.QuotationId }, quotation);
        }

        // PUT: api/Quotation/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuotation(Guid id, QuotationDto quotationDto)
        {
            if (id != quotationDto.QuotationId)
            {
                return BadRequest();
            }

            var updatedQuotation = await _quotationService.UpdateQuotationAsync(id, quotationDto);
            return Ok(updatedQuotation);
        }

        // POST: api/Quotation/5/send
        [HttpPost("{id}/send")]
        public async Task<ActionResult<QuotationDto>> SendQuotationToCustomer(Guid id)
        {
            var quotation = await _quotationService.SendQuotationToCustomerAsync(id);
            return Ok(quotation);
        }

        // POST: api/Quotation/5/approve
        [HttpPost("{id}/approve")]
        public async Task<ActionResult<QuotationDto>> ApproveQuotation(Guid id)
        {
            var quotation = await _quotationService.ApproveQuotationAsync(id);
            return Ok(quotation);
        }

        // POST: api/Quotation/5/reject
        [HttpPost("{id}/reject")]
        public async Task<ActionResult<QuotationDto>> RejectQuotation(Guid id, [FromBody] string rejectionReason)
        {
            var quotation = await _quotationService.RejectQuotationAsync(id, rejectionReason);
            return Ok(quotation);
        }

        // POST: api/Quotation/5/revise
        [HttpPost("{id}/revise")]
        public async Task<ActionResult<QuotationDto>> ReviseQuotation(Guid id, [FromBody] string revisionDetails)
        {
            var quotation = await _quotationService.ReviseQuotationAsync(id, revisionDetails);
            return Ok(quotation);
        }

        // DELETE: api/Quotation/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuotation(Guid id)
        {
            var result = await _quotationService.DeleteQuotationAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}