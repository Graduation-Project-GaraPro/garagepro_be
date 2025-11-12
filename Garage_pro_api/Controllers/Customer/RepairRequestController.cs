using BusinessObject.Customers;
using Dtos.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.Customer;
using System.ComponentModel.DataAnnotations;
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            // Lấy userId từ token
            try
            {              
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub"); // hoặc tên claim chứa idUser
                    if (string.IsNullOrEmpty(userId))
                        return Unauthorized();

                    var createdRequest = await _repairRequestService.CreateRepairRequestAsync(dto, userId);
                return Ok(createdRequest);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("arrival-availability/{branchId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetArrivalAvailability(Guid branchId, [FromQuery] DateOnly date)
        {
            try
            {
                var result = await _repairRequestService.GetArrivalAvailabilityAsync(branchId, date);
                return Ok(result);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost("withImage")]
        public async Task<IActionResult> CreateRepairRequestWithImage([FromForm] string dtoJson, [FromForm] List<IFormFile> images)
        {
            try
            {
                var dto = JsonConvert.DeserializeObject<CreateRepairRequestWithImageDto>(dtoJson);
                dto.Images = images;

                // ✅ Thực hiện validate thủ công
                var validationContext = new ValidationContext(dto);
                var validationResults = new List<ValidationResult>();

                if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
                {
                    return BadRequest(new
                    {
                        message = "Model validation failed",
                        errors = validationResults.Select(v => v.ErrorMessage)
                    });
                }

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
            var request = await _repairRequestService.GetByIdDetailsAsync(id);
            if (request == null)
                return NotFound();

            return Ok(request);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? vehicleId = null,
            [FromQuery] RepairRequestStatus? status = null,
            [FromQuery] Guid? branchId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _repairRequestService.GetPagedAsync(pageNumber, pageSize, vehicleId, status, branchId, userId);
            return Ok(result);
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


