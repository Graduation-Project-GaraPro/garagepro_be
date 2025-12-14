using BusinessObject;
using BusinessObject.Enums;
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
    //[Authorize]
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

        [HttpGet("{id}/details")]
        public async Task<ActionResult<QuotationDetailDto>> GetQuotationDetailById(Guid id)
        {
            try
            {
                var quotation = await _quotationService.GetQuotationDetailByIdAsync(id);
                if (quotation == null)
                    return NotFound();

                return Ok(quotation);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex);

            }
        }

        [HttpGet("inspection/{inspectionId}")]
        public async Task<ActionResult<IEnumerable<QuotationDto>>> GetQuotationsByInspectionId(Guid inspectionId)
        {
            var quotations = await _quotationService.GetQuotationsByInspectionIdAsync(inspectionId);
            return Ok(quotations);
        }

        [HttpGet("repair-order/{repairOrderId}")]
        public async Task<ActionResult<IEnumerable<QuotationDto>>> GetQuotationsByRepairOrderId(Guid repairOrderId)
        {
            try
            {
                var quotations = await _quotationService.GetQuotationsByRepairOrderIdAsync(repairOrderId);
                return Ok(quotations);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error in GetQuotationsByRepairOrderId: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, $"An error occurred while fetching quotations: {ex.Message}");
            }
        }

            [HttpGet("user")]
            public async Task<IActionResult> GetByUserId(
            
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] QuotationStatus? status = null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _quotationService.GetQuotationsByUserIdAsync(userId, pageNumber, pageSize, status);
                return Ok(result);
            }


        //[HttpGet("user")]
        //public async Task<ActionResult<IEnumerable<QuotationDto>>> GetQuotationsByUserId()
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (string.IsNullOrEmpty(userId))
        //        return BadRequest("User ID not found in token");

        //    var quotations = await _quotationService.GetQuotationsByUserIdAsync(userId);
        //    return Ok(quotations);
        //}

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

        // Copy approved quotation to jobs - Manager only
        [HttpPost("{id}/copy-to-jobs")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<bool>> CopyQuotationToJobs(Guid id)
        {
            Console.WriteLine($"CopyQuotationToJobs called with ID: {id}");
            try
            {
                var result = await _quotationService.CopyQuotationToJobsAsync(id);
                Console.WriteLine($"CopyQuotationToJobs completed with result: {result}");
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"CopyQuotationToJobs failed with ArgumentException: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"CopyQuotationToJobs failed with InvalidOperationException: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CopyQuotationToJobs failed with Exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        // Create revision jobs for an updated quotation - Manager only
        [HttpPost("{id}/create-revision-jobs")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<bool>> CreateRevisionJobs(Guid id, [FromBody] CreateRevisionJobsDto createRevisionJobsDto)
        {
            try
            {
                var result = await _quotationService.CreateRevisionJobsAsync(id, createRevisionJobsDto.RevisionReason);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
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

        // GET: api/quotations/can-complete-repair-order/{repairOrderId}
        [HttpGet("can-complete-repair-order/{repairOrderId}")]
        public async Task<ActionResult<bool>> CanCompleteRepairOrder(Guid repairOrderId)
        {
            try
            {
                var canComplete = await _quotationService.CanCompleteRepairOrderAsync(repairOrderId);
                return Ok(new { canComplete, repairOrderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred checking completion status", error = ex.Message });
            }
        }

        // POST: api/quotations/complete-repair-order/{repairOrderId}
        [HttpPost("complete-repair-order/{repairOrderId}")]
        [Authorize(Roles = "Manager")] // Only managers can complete repair orders
        public async Task<ActionResult> CompleteRepairOrder(Guid repairOrderId)
        {
            try
            {
                await _quotationService.CompleteRepairOrderWithGoodQuotationsAsync(repairOrderId);
                return Ok(new { message = "Repair order completed successfully", repairOrderId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred completing the repair order", error = ex.Message });
            }
        }
    }
}