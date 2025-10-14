using Dtos.Vehicles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.VehicleServices;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Garage_pro_api.Controllers.Vehicle
{
    [AllowAnonymous]
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in token");
            }
            
            var vehicles = await _vehicleService.GetUserVehiclesAsync(userId);
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

            // Check if UserID is provided in the DTO
            if (!string.IsNullOrEmpty(vehicleDto.UserID))
            {
                // Use the provided UserID directly
                var createdVehicle = await _vehicleService.CreateVehicleAsync(vehicleDto);
                return CreatedAtAction(nameof(GetVehicleById), new { id = createdVehicle.VehicleID }, createdVehicle);
            }
            else
            {
                // If no UserID provided, try to get it from the token (fallback for direct user vehicle creation)
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID not found in token and not provided in request");
                }
                
                var createdVehicle = await _vehicleService.CreateVehicleAsync(vehicleDto, userId);
                return CreatedAtAction(nameof(GetVehicleById), new { id = createdVehicle.VehicleID }, createdVehicle);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(Guid id, UpdateVehicleDto vehicleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedVehicle = await _vehicleService.UpdateVehicleAsync(id, vehicleDto);
            if (updatedVehicle == null)
                return NotFound();

            return Ok(updatedVehicle);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(Guid id)
        {
            var result = await _vehicleService.DeleteVehicleAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}