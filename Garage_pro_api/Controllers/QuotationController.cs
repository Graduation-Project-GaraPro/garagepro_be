using Dtos.Quotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.QuotationServices;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuotationsController : ControllerBase
    {
        private readonly IQuotationService _quotationService;

        public QuotationsController(IQuotationService quotationService)
        {
            _quotationService = quotationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuotationDto>>> GetAllQuotations()
        {
            var quotations = await _quotationService.GetAllQuotationsAsync();
            return Ok(quotations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuotationDto>> GetQuotationById(Guid id)
        {
            var quotation = await _quotationService.GetQuotationByIdAsync(id);
            if (quotation == null)
                return NotFound();

            return Ok(quotation);
        }

        [HttpGet("inspection/{inspectionId}")]
        public async Task<ActionResult<IEnumerable<QuotationDto>>> GetQuotationsByInspectionId(Guid inspectionId)
        {
            var quotations = await _quotationService.GetQuotationsByInspectionIdAsync(inspectionId);
            return Ok(quotations);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<QuotationDto>>> GetQuotationsByUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID not found in token");

            var quotations = await _quotationService.GetQuotationsByUserIdAsync(userId);
            return Ok(quotations);
        }

        [HttpPost]
        public async Task<ActionResult<QuotationDto>> CreateQuotation(CreateQuotationDto quotationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdQuotation = await _quotationService.CreateQuotationAsync(quotationDto);
            return CreatedAtAction(nameof(GetQuotationById), new { id = createdQuotation.QuotationId }, createdQuotation);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<QuotationDto>> UpdateQuotation(Guid id, UpdateQuotationDto quotationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedQuotation = await _quotationService.UpdateQuotationAsync(id, quotationDto);
                return Ok(updatedQuotation);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<QuotationDto>> UpdateQuotationStatus(Guid id, UpdateQuotationStatusDto statusDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedQuotation = await _quotationService.UpdateQuotationStatusAsync(id, statusDto);
                return Ok(updatedQuotation);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("customer-response")]
        public async Task<ActionResult<QuotationDto>> ProcessCustomerResponse(CustomerQuotationResponseDto responseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedQuotation = await _quotationService.ProcessCustomerResponseAsync(responseDto);
                return Ok(updatedQuotation);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteQuotation(Guid id)
        {
            var result = await _quotationService.DeleteQuotationAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}