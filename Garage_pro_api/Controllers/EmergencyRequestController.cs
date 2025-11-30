using Dtos.Emergency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.EmergencyRequestService;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Garage_pro_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class EmergencyRequestController : ControllerBase
    {
        private readonly IEmergencyRequestService _service;

        public EmergencyRequestController(IEmergencyRequestService service)
        {
            _service = service;
        }

        /// <summary>
        ///  Tìm các gara gần nhất theo tọa độ.
        /// </summary>
        [HttpGet("nearby-branches")]
        public async Task<IActionResult> GetNearestBranches([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] int count = 5)
        {
            var result = await _service.GetNearestBranchesAsync(latitude, longitude, count);
            return Ok(result);
        }

        [HttpPost("nearby-branches")]
        public async Task<IActionResult> GetNearestBranchesPost([FromBody] NearbyBranchRequestDto location)
        {
            if (location == null)
                return BadRequest("Invalid location data.");

            var result = await _service.GetNearestBranchesAsync(location.Latitude, location.Longitude, 5);
            return Ok(result);
        }

        /// <summary>
        /// Tạo yêu cầu cứu hộ mới.
        /// </summary>
        [HttpPost("create")]
        [Authorize(Roles = "Customer")] // Chỉ khách hàng đã đăng nhập mới gửi được
        public async Task<IActionResult> CreateEmergency([FromBody] CreateEmergencyRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Lấy userId từ JWT token
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? User.FindFirstValue("sub"); 
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not found in token.");

                // Gọi service và truyền userId
                var idempotencyKey = Request.Headers["Idempotency-Key"].ToString();
                var result = await _service.CreateEmergencyAsync(userId, dto, idempotencyKey);
                
                return CreatedAtAction(nameof(GetById), new { id = result.EmergencyRequestId }, result);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                    return BadRequest(new { message = ex.Message });
                if (ex is InvalidOperationException && ex.Message.Contains("Active emergency", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { message = ex.Message });
             //   if (ex is InvalidOperationException && ex.Message.Contains("Too many requests", StringComparison.OrdinalIgnoreCase))
             //       return StatusCode(429, new { message = ex.Message });
                var inner = ex.InnerException;
                while (inner != null)
                {
                    Console.WriteLine($"Inner: {inner.Message}");
                    inner = inner.InnerException;
                }

                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
            }

        }

        [HttpPost("inprogress/{emergenciesId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SetInProgress(Guid emergenciesId)
        {
            try
            {
                var ok = await _service.SetInProgressAsync(emergenciesId);
                return Ok(new { Success = ok });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("cancel/{emergenciesId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CancelEmergency(Guid emergenciesId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not found in token.");

                var ok = await _service.CancelEmergencyAsync(userId, emergenciesId);
                return Ok(new { Success = ok });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAllRequestEmergencies()
        {
            var request = await _service.GetAllRequestEmergencyAsync();
            return Ok(request);
        }

        /// <summary>
        /// Lấy danh sách yêu cầu cứu hộ của khách hàng.
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
                return BadRequest("Customer ID is required.");

            var requests = await _service.GetByCustomerAsync(customerId);
            return Ok(requests);
        }

        /// <summary>
        ///  Lấy chi tiết yêu cầu cứu hộ theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var dto = await _service.GetDtoByIdAsync(id);
            if (dto == null)
                return NotFound("Emergency request not found.");

            return Ok(dto);
        }

        [HttpGet("address")]
        public async Task<IActionResult> GetAddress([FromQuery] double latitude, [FromQuery] double longitude)
        {
            try
            {
                var address = await _service.ReverseGeocodeAddressAsync(latitude, longitude);
                var mapUrl = $"https://www.google.com/maps?q={latitude},{longitude}";
                return Ok(new { address, latitude, longitude, mapUrl });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("route")]
        public async Task<IActionResult> GetRoute([FromQuery] double fromLat, [FromQuery] double fromLon, [FromQuery] double toLat, [FromQuery] double toLon)
        {
            try
            {
                var route = await _service.GetRouteAsync(fromLat, fromLon, toLat, toLon);
                return Ok(route);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (TaskCanceledException ex)
            {
                return StatusCode(504, new { message = "Routing timeout", detail = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { message = "Routing service error", detail = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { message = "Routing unexpected error", detail = ex.Message });
            }
        }

        [HttpGet("route/{id}")]
        public async Task<IActionResult> GetRouteByEmergencyId(Guid id)
        {
            try
            {
                var route = await _service.GetRouteByEmergencyIdAsync(id);
                return Ok(route);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { message = "Routing service error", detail = ex.Message });
            }
        }
        // approve emergency
        
        [HttpPost("approve/{emergenciesId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ApproveEmergency(Guid emergenciesId)
        {
            try
            {
                var managerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (string.IsNullOrEmpty(managerUserId))
                    return Unauthorized("User not found in token.");

                var result = await _service.ApproveEmergency(emergenciesId, managerUserId);
                return Ok(new { Success = result });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException dbEx)
            {
                // Log chi tiết lỗi database để debug
                Console.WriteLine($"Database error in ApproveEmergency: {dbEx.Message}");
                if (dbEx.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                }
                return StatusCode(500, $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi để debug
                Console.WriteLine($"Error in ApproveEmergency: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, $"Unhandled error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
        // reject emergency
        [HttpPut("reject/{emergencyId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> RejectEmergency(Guid emergencyId, [FromBody] string? reason)
        {
            try
            {
                var result = await _service.RejectEmergency(emergencyId, reason);
                return Ok(new { Success = result });
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


    }
}
