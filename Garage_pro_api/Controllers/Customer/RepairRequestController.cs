using Dtos.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Customer;
using System.Security.Claims;

namespace Garage_pro_api.Controllers.Customer
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RepairRequestController : ControllerBase
    {
        private readonly IRepairRequestService _repairRequestService;

        public RepairRequestController(IRepairRequestService repairRequestService)
        {
            _repairRequestService = repairRequestService;
        }

        // POST: api/RepairRequests
        [HttpPost]
        public async Task<IActionResult> CreateRepairRequest([FromBody] CreateRequestDto dto)
        {
            // Lấy userId từ token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
             ?? User.FindFirstValue("sub"); // hoặc tên claim chứa idUser
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var createdRequest = await _repairRequestService.CreateRepairRequestAsync(dto, userId);
            return Ok(createdRequest);
    
        }

        // GET: api/RepairRequests/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRepairRequestById(Guid id)
        {
            var request = await _repairRequestService.GetByIdAsync(id);
            if (request == null)
                return NotFound();

            return Ok(request);
        }


        // GET: api/RepairRequests
        [HttpGet("user-requests")]
        public async Task<IActionResult> GetUserRepairRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
             ?? User.FindFirstValue("sub"); // hoặc "idUser"
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();// hoặc tên claim chứa idUser
            var requests = await _repairRequestService.GetByUserIdAsync(userId);
            return Ok(requests);
        }
    }
}


