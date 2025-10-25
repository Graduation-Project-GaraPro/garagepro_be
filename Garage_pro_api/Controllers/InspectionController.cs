using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services;
using Dtos.Quotations;
using System.Security.Claims;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InspectionsController : ControllerBase
    {
        private readonly IInspectionService _inspectionService;

        public InspectionsController(IInspectionService inspectionService)
        {
            _inspectionService = inspectionService;
        }

        // GET: api/Inspections
        [HttpGet]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<ActionResult<IEnumerable<InspectionDto>>> GetAllInspections()
        {
            var inspections = await _inspectionService.GetAllInspectionsAsync();
            return Ok(inspections);
        }

        // GET: api/Inspections/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<ActionResult<InspectionDto>> GetInspectionById(Guid id)
        {
            var inspection = await _inspectionService.GetInspectionByIdAsync(id);
            if (inspection == null)
            {
                return NotFound();
            }

            return Ok(inspection);
        }

        // GET: api/Inspections/repairorder/{repairOrderId}
        [HttpGet("repairorder/{repairOrderId}")]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<ActionResult<IEnumerable<InspectionDto>>> GetInspectionsByRepairOrderId(Guid repairOrderId)
        {
            var inspections = await _inspectionService.GetInspectionsByRepairOrderIdAsync(repairOrderId);
            return Ok(inspections);
        }

        // GET: api/Inspections/technician/{technicianId}
        [HttpGet("technician/{technicianId}")]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<ActionResult<IEnumerable<InspectionDto>>> GetInspectionsByTechnicianId(Guid technicianId)
        {
            var inspections = await _inspectionService.GetInspectionsByTechnicianIdAsync(technicianId);
            return Ok(inspections);
        }

        // GET: api/Inspections/pending
        [HttpGet("pending")]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<ActionResult<IEnumerable<InspectionDto>>> GetPendingInspections()
        {
            var inspections = await _inspectionService.GetPendingInspectionsAsync();
            return Ok(inspections);
        }

        // GET: api/Inspections/completed
        [HttpGet("completed")]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<ActionResult<IEnumerable<InspectionDto>>> GetCompletedInspections()
        {
            var inspections = await _inspectionService.GetCompletedInspectionsAsync();
            return Ok(inspections);
        }

        // POST: api/Inspections
        [HttpPost]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<ActionResult<InspectionDto>> CreateInspection(CreateInspectionDto createInspectionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdInspection = await _inspectionService.CreateInspectionAsync(createInspectionDto);
                return CreatedAtAction(nameof(GetInspectionById), new { id = createdInspection.InspectionId }, createdInspection);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Inspections/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<ActionResult<InspectionDto>> UpdateInspection(Guid id, UpdateInspectionDto updateInspectionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedInspection = await _inspectionService.UpdateInspectionAsync(id, updateInspectionDto);
                return Ok(updatedInspection);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Inspections/{id}/assign/{technicianId}
        [HttpPut("{id}/assign/{technicianId}")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<ActionResult> AssignInspectionToTechnician(Guid id, Guid technicianId)
        {
            var result = await _inspectionService.AssignInspectionToTechnicianAsync(id, technicianId);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Inspections/{id}/finding
        [HttpPut("{id}/finding")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<ActionResult> UpdateInspectionFinding(Guid id, [FromBody] UpdateInspectionFindingDto findingDto)
        {
            var result = await _inspectionService.UpdateInspectionFindingAsync(id, findingDto.Finding, findingDto.Note, findingDto.IssueRating);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Inspections/{id}/concern
        [HttpPut("{id}/concern")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<ActionResult> UpdateCustomerConcern(Guid id, [FromBody] UpdateCustomerConcernDto concernDto)
        {
            var result = await _inspectionService.UpdateCustomerConcernAsync(id, concernDto.CustomerConcern);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Inspections/{id}/price
        [HttpPut("{id}/price")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<ActionResult> UpdateInspectionPrice(Guid id, [FromBody] UpdateInspectionPriceDto priceDto)
        {
            var result = await _inspectionService.UpdateInspectionPriceAsync(id, priceDto.InspectionPrice);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Inspections/{id}/type
        [HttpPut("{id}/type")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<ActionResult> UpdateInspectionType(Guid id, [FromBody] UpdateInspectionTypeDto typeDto)
        {
            var result = await _inspectionService.UpdateInspectionTypeAsync(id, typeDto.InspectionType);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Inspections/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<ActionResult> DeleteInspection(Guid id)
        {
            var result = await _inspectionService.DeleteInspectionAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }

    // Additional DTOs for specific operations
    public class UpdateInspectionFindingDto
    {
        public string Finding { get; set; }
        public string Note { get; set; }
        public BusinessObject.Enums.IssueRating IssueRating { get; set; }
    }

    public class UpdateCustomerConcernDto
    {
        public string CustomerConcern { get; set; }
    }

    public class UpdateInspectionPriceDto
    {
        public decimal InspectionPrice { get; set; }
    }
    
    public class UpdateInspectionTypeDto
    {
        public BusinessObject.Enums.InspectionType InspectionType { get; set; }
    }
}