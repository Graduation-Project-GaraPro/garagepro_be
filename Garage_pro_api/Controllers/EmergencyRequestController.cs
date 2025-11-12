using Dtos.Emergency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> GetNearestBranches([FromBody] NearbyBranchRequestDto location)
        {
            if (location == null)
                return BadRequest("Invalid location data.");

            // Gọi service với latitude + longitude trực tiếp
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
                             ?? User.FindFirstValue("sub"); // hoặc claim chứa ID khách hàng
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not found in token.");

                // Gọi service và truyền userId
                var result = await _service.CreateEmergencyAsync(userId, dto);

                // Trả về CreatedAtAction với route GetById
                return CreatedAtAction(nameof(GetById), new { id = result.EmergencyRequestId }, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, $"An error occurred: {ex.Message}");

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
            var request = await _service.GetByIdAsync(id);
            if (request == null)
                return NotFound("Emergency request not found.");

            return Ok(request);
        }
        // approve emergency
        [HttpPost("approve/{emergenciesId}")]
        public async Task<IActionResult> ApproveEmergency(Guid emergenciesId)
        {
            try
            {
                var result = await _service.ApproveEmergency(emergenciesId);
                return Ok(new { Success = result });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        // reject emergency
        [HttpPut("reject/{emergencyId}")]
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
