
using Dtos.Vehicles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.VehicleServices;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Garage_pro_api.Controllers.Vehicle
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVehicles()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return Ok(vehicles);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserVehicles()
        {
            // Lấy userId từ token
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? User.FindFirstValue("sub"); // hoặc tên claim chứa idUser
            if (string.IsNullOrEmpty(UserId))
                return Unauthorized();
            var vehicles = await _vehicleService.GetUserVehiclesAsync(UserId);
            return Ok(vehicles);
        }

        [HttpGet("user/selectable")]
        public async Task<IActionResult> GetUserVehiclesSelectable()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var vehicles = await _vehicleService.GetUserVehiclesSelectableAsync(userId);
            return Ok(vehicles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(Guid id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null)
                return NotFound();

            return Ok(vehicle);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicle(CreateVehicleDto vehicleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Lấy userId từ token
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? User.FindFirstValue("sub"); // hoặc tên claim chứa idUser
            if (string.IsNullOrEmpty(UserId))
                return Unauthorized();


            try
            {
                var createdVehicle = await _vehicleService.CreateVehicleAsync(vehicleDto, UserId);
                return CreatedAtAction(nameof(GetVehicleById), new { id = createdVehicle.VehicleID }, createdVehicle);
            }
            catch (Exception ex)
            {
                var msg = ex.Message ?? string.Empty;
                if (msg.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { message = msg });
                return StatusCode(500, new { message = msg });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(Guid id, UpdateVehicleDto vehicleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var updatedVehicle = await _vehicleService.UpdateVehicleAsync(id, vehicleDto);
                if (updatedVehicle == null)
                    return NotFound();

                return Ok(updatedVehicle);
            }
            catch (ApplicationException ex)
            {
                return Conflict(new { message = ex.Message, actionable = true });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật xe.", detail = ex.Message });
            }
        }

        [HttpPost("customer")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> CreateVehicleForCustomer(CreateVehicleForCustomerDto vehicleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdVehicle = await _vehicleService.CreateVehicleForCustomerAsync(vehicleDto);
                return CreatedAtAction(nameof(GetVehicleById), new { id = createdVehicle.VehicleID }, createdVehicle);
            }
            catch (Exception ex)
            {
                var msg = ex.Message ?? string.Empty;
                if (msg.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { message = msg });
                return StatusCode(500, new { message = msg });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(Guid id)
        {
            try
            {
                var result = await _vehicleService.DeleteVehicleAsync(id);
                if (!result)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                var msg = ex.Message ?? string.Empty;
                if (msg.Contains("Cannot delete vehicle", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { message = msg });
                return StatusCode(500, new { message = msg });
            }
        }
    }
}