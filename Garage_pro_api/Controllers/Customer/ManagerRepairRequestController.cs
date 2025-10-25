using Dtos.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Services.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // GET: api/ManagerRepairRequest/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRepairRequestById(Guid id)
        {
            var request = await _repairRequestService.GetManagerRequestByIdAsync(id);
            
            if (request == null)
                return NotFound();

            return Ok(request);
        }
    }
}