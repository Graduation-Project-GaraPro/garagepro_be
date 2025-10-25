using Dtos.Vehicles;
using Microsoft.AspNetCore.Mvc;
using Services.VehicleServices;
using Garage_pro_api.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleIntegrationController : ControllerBase
    {
        private readonly IVehicleIntegrationService _vehicleIntegrationService;

        public VehicleIntegrationController(IVehicleIntegrationService vehicleIntegrationService)
        {
            _vehicleIntegrationService = vehicleIntegrationService;
        }

        // GET: api/VehicleIntegration/vehicle/{vehicleId}/history
        [HttpGet("vehicle/{vehicleId}/history")]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<IActionResult> GetVehicleWithServiceHistory(Guid vehicleId)
        {
            var vehicleWithHistory = await _vehicleIntegrationService.GetVehicleWithServiceHistoryAsync(vehicleId);
            if (vehicleWithHistory == null)
            {
                return NotFound();
            }

            return Ok(vehicleWithHistory);
        }

        // GET: api/VehicleIntegration/vehicle/{vehicleId}
        [HttpGet("vehicle/{vehicleId}")]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<IActionResult> GetVehicleWithCustomer(Guid vehicleId)
        {
            var vehicle = await _vehicleIntegrationService.GetVehicleWithCustomerAsync(vehicleId);
            if (vehicle == null)
            {
                return NotFound();
            }

            return Ok(vehicle);
        }

        // GET: api/VehicleIntegration/customer/{userId}/vehicles
        [HttpGet("customer/{userId}/vehicles")]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<IActionResult> GetVehiclesForCustomer(string userId)
        {
            var vehicles = await _vehicleIntegrationService.GetVehiclesForCustomerAsync(userId);
            return Ok(vehicles);
        }

        // GET: api/VehicleIntegration/vehicle/{vehicleId}/scheduling
        [HttpGet("vehicle/{vehicleId}/scheduling")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<IActionResult> GetVehicleSchedulingInfo(Guid vehicleId)
        {
            var schedulingInfo = await _vehicleIntegrationService.GetVehicleSchedulingInfoAsync(vehicleId);
            if (schedulingInfo == null)
            {
                return NotFound();
            }

            return Ok(schedulingInfo);
        }

        // PUT: api/VehicleIntegration/vehicle/{vehicleId}/schedule
        [HttpPut("vehicle/{vehicleId}/schedule")]
        [Authorize(Policy = "BOOKING_MANAGE")]
        public async Task<IActionResult> UpdateVehicleSchedule(Guid vehicleId, [FromBody] DateTime? nextServiceDate)
        {
            var result = await _vehicleIntegrationService.UpdateVehicleScheduleAsync(vehicleId, nextServiceDate);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // GET: api/VehicleIntegration/vehicle/{vehicleId}/insurance
        [HttpGet("vehicle/{vehicleId}/insurance")]
        [Authorize(Policy = "BOOKING_VIEW")]
        public async Task<IActionResult> GetVehicleInsuranceInfo(Guid vehicleId)
        {
            var insuranceInfo = await _vehicleIntegrationService.GetVehicleInsuranceInfoAsync(vehicleId);
            if (insuranceInfo == null)
            {
                return NotFound();
            }

            return Ok(insuranceInfo);
        }
    }
}