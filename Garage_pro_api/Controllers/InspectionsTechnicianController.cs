using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Enums;
using Dtos.InspectionAndRepair;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.InspectionAndRepair;

namespace Garage_pro_api.Controllers
{
    [Route("odata/[controller]")]
    [ApiController]
    public class InspectionsTechnicianController : ODataController
    {
        private readonly IInspectionTechnicianService _inspectionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public InspectionsTechnicianController(IInspectionTechnicianService inspectionService, UserManager<ApplicationUser> userManager)
        {
            _inspectionService = inspectionService;
            _userManager = userManager;
        }

        [HttpGet("my-inspections")]
        [Authorize("INSPECTION_TECHNICIAN_VIEW")]
        public async Task<IActionResult> GetMyInspections()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { Message = "You need to log in to view the vehicle inspection list." });

            var inspections = await _inspectionService.GetInspectionsByTechnicianAsync(user.Id);
            if (inspections == null || !inspections.Any())
                return Ok(new { Message = "You have not been assigned to inspect any vehicles yet." });

            return Ok(inspections); 
        }

        [HttpGet("{id}")]
        [Authorize("INSPECTION_TECHNICIAN_VIEW")]
        public async Task<IActionResult> GetInspectionById(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { Message = "You need to log in." });

            var dto = await _inspectionService.GetInspectionByIdAsync(id, user.Id);
            if (dto == null) return NotFound(new { Message = "Vehicle inspection not found or you do not have permission to access it." });

            return Ok(dto);
        }

        [HttpPut("{id}")]
        [Authorize("INSPECTION_TECHNICIAN_UPDATE")]
        public async Task<IActionResult> UpdateInspection(Guid id, [FromBody] UpdateInspectionRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { Message = "You need to log in." });

            try
            {
                var updatedDto = await _inspectionService.UpdateInspectionAsync(id, request, user.Id);
                return Ok(new
                {
                    Message = request.IsCompleted ? "Inspection completed and sent for review." : "Vehicle inspection updated successfully.",
                    Inspection = updatedDto
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the vehicle inspection.", Error = ex.Message });
            }
        }
        [HttpDelete("{inspectionId}/services/{serviceId}/part-inspections/{partInspectionId}")]
        [Authorize("INSPECTION_TECHNICIAN_DELETE")]
        public async Task<IActionResult> RemovePartFromInspection(Guid inspectionId, Guid serviceId, Guid partInspectionId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "You need to log in." });

            try
            {
                await _inspectionService.RemovePartFromInspectionAsync(inspectionId, serviceId, partInspectionId, user.Id);
                return Ok(new { Message = "The part was successfully removed from the inspection." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while removing the part from the inspection.", Error = ex.Message });
            }
        }

        [HttpDelete("{inspectionId}/services/{serviceInspectionId}/part-categories/{partCategoryId}")]
        [Authorize("INSPECTION_TECHNICIAN_DELETE")]
        public async Task<IActionResult> RemovePartCategoryFromService(
            Guid inspectionId,
            Guid serviceInspectionId,
            Guid partCategoryId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "You need to log in." });

            try
            {
                var dto = await _inspectionService.RemovePartCategoryFromServiceAsync(
                    inspectionId,
                    serviceInspectionId,
                    partCategoryId,
                    user.Id
                );

                return Ok(new
                {
                    Message = "The part category and its related parts were successfully deleted.",
                    Inspection = dto
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while deleting the PartCategory.",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("{id}/start")]
        [Authorize("INSPECTION_TECHNICIAN_UPDATE")]
        public async Task<IActionResult> StartInspection(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { Message = "You need to log in." });

            try
            {
                var dto = await _inspectionService.StartInspectionAsync(id, user.Id);
                return Ok(new { Message = "The vehicle inspection has started.", Inspection = dto });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error !", Error = ex.Message });
            }
        }

        [HttpGet("services")]
        [Authorize("INSPECTION_TECHNICIAN_VIEW")]
        public async Task<IActionResult> GetAllServices()
        {
            try
            {
                var services = await _inspectionService.GetAllServicesAsync();
                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the service list.", Error = ex.Message });
            }
        }

        [HttpPost("{inspectionId}/services")]
        [Authorize("INSPECTION_ADD_SERVICE")]
        public async Task<IActionResult> AddServiceToInspection(Guid inspectionId, [FromBody] AddServiceToInspectionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "You need to log in." });

            try
            {
                var dto = await _inspectionService.AddServiceToInspectionAsync(inspectionId, request, user.Id);
                return Ok(new
                {
                    Message = "Service was successfully added to the inspection.",
                    Inspection = dto
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while adding the service to the inspection.", Error = ex.Message });
            }
        }

        [HttpDelete("{inspectionId}/services/{serviceInspectionId}")]
        [Authorize("INSPECTION_TECHNICIAN_DELETE")]
        public async Task<IActionResult> RemoveServiceFromInspection(Guid inspectionId, Guid serviceInspectionId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "You need to log in." });

            try
            {
                var dto = await _inspectionService.RemoveServiceFromInspectionAsync(inspectionId, serviceInspectionId, user.Id);
                return Ok(new
                {
                    Message = "Service removed from the inspection successfully.",
                    Inspection = dto
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while removing the service from the inspection.", Error = ex.Message });
            }
        }
        [HttpGet("my-technician-id")]
        [Authorize("INSPECTION_TECHNICIAN_VIEW")]
        public async Task<IActionResult> GetMyTechnicianId()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "You need to log in." });

            var technician = await _inspectionService.GetTechnicianByUserIdAsync(user.Id);
            if (technician == null)
                return NotFound(new { Message = "Technician information not found." });

            return Ok(new { TechnicianId = technician.TechnicianId });
        }

    }
}