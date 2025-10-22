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
        public async Task<IActionResult> CreateRepairRequest([FromBody] CreateRepairRequestWithImageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            // Lấy userId từ token
            try
            {              
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub"); // hoặc tên claim chứa idUser
                    if (string.IsNullOrEmpty(userId))
                        return Unauthorized();

                    var createdRequest = await _repairRequestService.CreateRepairWithImageRequestAsync(dto, userId);
                return Ok(createdRequest);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("withImage")]
        public async Task<IActionResult> CreateRepairRequestWithImage([FromForm] CreateRepairRequestWithImageDto dto)
        {
           

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _repairRequestService.CreateRepairWithImageRequestAsync(dto, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //Put: api/RepairRequest/{requestId}
        [HttpPut("{requestId}")]
        public async Task<IActionResult> UpdateRepairRequest(Guid requestId, [FromBody] UpdateRepairRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? User.FindFirstValue("sub"); // hoặc tên claim chứa idUser
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();
                // Gọi service update
                var updatedRequest = await _repairRequestService.UpdateRepairRequestAsync(requestId, dto, userId);
                //  Lấy lại repairRequest vừa update
                //var updatedRequest = await _repairRequestService.GetByIdAsync(requestId);           
                // Trả về client
                return Ok(updatedRequest);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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


