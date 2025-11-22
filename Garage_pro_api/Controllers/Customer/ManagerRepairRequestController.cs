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

        // PUT: api/ManagerRepairRequest/{id}/approve
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveRepairRequest(Guid id)
        {
            var result = await _repairRequestService.ApproveRepairRequestAsync(id);
            
            if (!result)
                return BadRequest("Failed to approve repair request. Request may not exist or is not in pending status.");

            return Ok(new { Message = "Repair request approved successfully" });
        }

        // PUT: api/ManagerRepairRequest/{id}/reject
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectRepairRequest(Guid id)
        {
            var result = await _repairRequestService.RejectRepairRequestAsync(id);
            
            if (!result)
                return BadRequest("Failed to reject repair request. Request may not exist or is not in pending status.");

            return Ok(new { Message = "Repair request rejected successfully" });
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
    }
}