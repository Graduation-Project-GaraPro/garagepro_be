using Dtos.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Services.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dtos.RepairOrder;

namespace Garage_pro_api.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager")] // Only managers can access
    public class ManagerRepairRequestController : ControllerBase
    {
        private readonly IRepairRequestService _repairRequestService;

        public ManagerRepairRequestController(IRepairRequestService repairRequestService)
        {
            _repairRequestService = repairRequestService;
        }

        // GET: api/ManagerRepairRequest
        [HttpGet]
        [EnableQuery] // Enable OData query support
        public async Task<IActionResult> GetRepairRequests()
        {
            var requests = await _repairRequestService.GetForManagerAsync();
            return Ok(requests);
        }

        // GET: api/ManagerRepairRequest/branch/{branchId}
        [HttpGet("branch/{branchId}")]
        [EnableQuery] // Enable OData query support
        public async Task<IActionResult> GetRepairRequestsByBranch(Guid branchId)
        {
            var requests = await _repairRequestService.GetForManagerByBranchAsync(branchId);
            return Ok(requests);
        }

        // GET: api/ManagerRepairRequest/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRepairRequestById(Guid id)
        {
            var request = await _repairRequestService.GetManagerRequestByIdAsync(id);
            
            if (request == null)
                return NotFound();

            return Ok(request);
        }


        // POST: api/ManagerRepairRequest/{id}/convert-to-ro
        [HttpPost("{id}/convert-to-ro")]
        public async Task<IActionResult> ConvertToRepairOrder(Guid id, [FromBody] CreateRoFromRequestDto dto)
        {
            try
            {
                var repairOrderDto = await _repairRequestService.ConvertToRepairOrderAsync(id, dto);
                return Ok(repairOrderDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: api/ManagerRepairRequest/{id}/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelRepairRequest(Guid id)
        {
            try
            {
                var managerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(managerId))
                    return Unauthorized(new { Message = "Manager ID not found in token" });

                var result = await _repairRequestService.ManagerCancelRepairRequestAsync(id, managerId);
                return Ok(new { Message = "Repair request cancelled successfully", Success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}