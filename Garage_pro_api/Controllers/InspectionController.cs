using Dtos.Quotations;
using Garage_pro_api.DbInit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InspectionController : ControllerBase
    {
        private readonly IInspectionService _inspectionService;

        public InspectionController(IInspectionService inspectionService)
        {
            _inspectionService = inspectionService;
        }

        // GET: api/Inspections
        [HttpGet]
        [Authorize(Policy = "JOB_VIEW")]
        public async Task<ActionResult<IEnumerable<InspectionDto>>> GetAllInspections()
        {
            var inspections = await _inspectionService.GetAllInspectionsAsync();
            return Ok(inspections);
        }

        // GET: api/Inspections/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "JOB_VIEW")]
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
        [Authorize(Policy = "JOB_VIEW")]
        public async Task<ActionResult<IEnumerable<InspectionDto>>> GetInspectionsByRepairOrderId(Guid repairOrderId)
        {
            var inspections = await _inspectionService.GetInspectionsByRepairOrderIdAsync(repairOrderId);
            return Ok(inspections);
        }

        // GET: api/Inspections/technician/{technicianId}
        [HttpGet("technician/{technicianId}")]
        [Authorize(Policy = "JOB_VIEW")]
        public async Task<ActionResult<IEnumerable<InspectionDto>>> GetInspectionsByTechnicianId(Guid technicianId)
        {
            var inspections = await _inspectionService.GetInspectionsByTechnicianIdAsync(technicianId);
            return Ok(inspections);
        }

        // GET: api/Inspections/pending
        [HttpGet("pending")]
        [Authorize(Policy = "JOB_VIEW")]
        public async Task<ActionResult<IEnumerable<InspectionDto>>> GetPendingInspections()
        {
            var inspections = await _inspectionService.GetPendingInspectionsAsync();
            return Ok(inspections);
        }

        // GET: api/Inspections/completed
        [HttpGet("completed")]
        [Authorize(Policy = "JOB_VIEW")]
        public async Task<ActionResult<IEnumerable<InspectionDto>>> GetCompletedInspections()
        {
            var inspections = await _inspectionService.GetCompletedInspectionsAsync();
            return Ok(inspections);
        }

        // GET: api/Inspections/completed-with-details
        [HttpGet("completed-with-details")]
        [Authorize(Policy = "JOB_VIEW")]
        public async Task<ActionResult<IEnumerable<CompletedInspectionDto>>> GetCompletedInspectionsWithDetails()
        {
            var inspections = await _inspectionService.GetCompletedInspectionsWithDetailsAsync();
            return Ok(inspections);
        }

        // POST: api/Inspections
        [HttpPost]
        [Authorize(Policy = "JOB_MANAGE")]
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

        // POST: api/Inspections/convert-to-quotation
        [HttpPost("convert-to-quotation")]
        //[Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<ActionResult<QuotationDto>> ConvertInspectionToQuotation([FromBody] ConvertInspectionToQuotationDto convertDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var quotation = await _inspectionService.ConvertInspectionToQuotationAsync(convertDto);
                return Ok(quotation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while converting inspection to quotation", error = ex.Message });
            }
        }

        // PUT: api/Inspections/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "JOB_MANAGE")]
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
        [Authorize(Policy = "JOB_MANAGE")]
        public async Task<ActionResult> AssignInspectionToTechnician(Guid id, Guid technicianId)
        {
            var result = await _inspectionService.AssignInspectionToTechnicianAsync(id, technicianId);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Inspections/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "JOB_MANAGE")]
        public async Task<ActionResult> DeleteInspection(Guid id)
        {
            var result = await _inspectionService.DeleteInspectionAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Inspection/seed-database
        [HttpPost("seed-database")]
        [Authorize(Policy = "JOB_MANAGE")]
        public async Task<ActionResult> SeedDatabase([FromServices] IServiceProvider serviceProvider)
        {
            try
            {
                var dbInitializer = serviceProvider.GetRequiredService<DbInitializer>();
                await dbInitializer.Initialize();
                return Ok(new { message = "Database seeding completed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while seeding the database", error = ex.Message });
            }
        }

        // GET: api/Inspection/check-seeding
        [HttpGet("check-seeding")]
        [Authorize(Policy = "JOB_VIEW")]
        public async Task<ActionResult> CheckSeeding([FromServices] IInspectionService inspectionService)
        {
            try
            {
                var inspections = await inspectionService.GetAllInspectionsAsync();
                var count = inspections.Count();
                return Ok(new { message = $"Database has {count} inspections seeded", inspectionCount = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking seeding status", error = ex.Message });
            }
        }
    }
}