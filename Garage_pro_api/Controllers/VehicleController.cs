using Dtos.Vehicle;
using Microsoft.AspNetCore.Mvc;
using Services.VehicleServices;
using Garage_pro_api.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        // GET: api/Vehicle
        [HttpGet]
        [Authorize(Policy = "VEHICLE_VIEW")]
        public async Task<IActionResult> GetAllVehicles()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return Ok(vehicles);
        }

        // GET: api/Vehicle/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "VEHICLE_VIEW")]
        public async Task<IActionResult> GetVehicle(Guid id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return Ok(vehicle);
        }

        // GET: api/Vehicle/vin/{vin}
        [HttpGet("vin/{vin}")]
        [Authorize(Policy = "VEHICLE_VIEW")]
        public async Task<IActionResult> GetVehicleByVin(string vin)
        {
            var vehicle = await _vehicleService.GetVehicleByVinAsync(vin);
            if (vehicle == null)
            {
                return NotFound();
            }

            return Ok(vehicle);
        }

        // GET: api/Vehicle/licenseplate/{licensePlate}
        [HttpGet("licenseplate/{licensePlate}")]
        [Authorize(Policy = "VEHICLE_VIEW")]
        public async Task<IActionResult> GetVehicleByLicensePlate(string licensePlate)
        {
            var vehicle = await _vehicleService.GetVehicleByLicensePlateAsync(licensePlate);
            if (vehicle == null)
            {
                return NotFound();
            }

            return Ok(vehicle);
        }

        // GET: api/Vehicle/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize(Policy = "VEHICLE_VIEW")]
        public async Task<IActionResult> GetVehiclesByUserId(string userId)
        {
            var vehicles = await _vehicleService.GetVehiclesByUserIdAsync(userId);
            return Ok(vehicles);
        }

        // GET: api/Vehicle/{id}/details
        [HttpGet("{id}/details")]
        [Authorize(Policy = "VEHICLE_VIEW")]
        public async Task<IActionResult> GetVehicleWithCustomer(Guid id)
        {
            var vehicleWithCustomer = await _vehicleService.GetVehicleWithCustomerAsync(id);
            if (vehicleWithCustomer == null)
            {
                return NotFound();
            }

            return Ok(vehicleWithCustomer);
        }

        // POST: api/Vehicle
        [HttpPost]
        [Authorize(Policy = "VEHICLE_CREATE")]
        public async Task<IActionResult> CreateVehicle(CreateVehicleDto createVehicleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vehicle = await _vehicleService.CreateVehicleAsync(createVehicleDto);
            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.VehicleId }, vehicle);
        }

        // PUT: api/Vehicle/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "VEHICLE_EDIT")]
        public async Task<IActionResult> UpdateVehicle(Guid id, UpdateVehicleDto updateVehicleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var vehicle = await _vehicleService.UpdateVehicleAsync(id, updateVehicleDto);
                return Ok(vehicle);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // PUT: api/Vehicle/{id}/warranty
        [HttpPut("{id}/warranty")]
        [Authorize(Policy = "VEHICLE_WARRANTY")]
        public async Task<IActionResult> UpdateWarrantyStatus(Guid id, [FromBody] string warrantyStatus)
        {
            var result = await _vehicleService.UpdateWarrantyStatusAsync(id, warrantyStatus);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Vehicle/{id}/schedule
        [HttpPut("{id}/schedule")]
        [Authorize(Policy = "VEHICLE_SCHEDULE")]
        public async Task<IActionResult> UpdateServiceSchedule(Guid id, [FromBody] DateTime? nextServiceDate)
        {
            var result = await _vehicleService.UpdateServiceScheduleAsync(id, nextServiceDate);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Vehicle/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "VEHICLE_DELETE")]
        public async Task<IActionResult> DeleteVehicle(Guid id)
        {
            var result = await _vehicleService.DeleteVehicleAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}