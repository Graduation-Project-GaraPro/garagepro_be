using Dtos.Vehicles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.VehicleServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // nếu muốn chỉ user đăng nhập mới truy cập
    public class VehicleColorsController : ControllerBase
    {
        private readonly IVehicleColorService _vehicleColorService;

        public VehicleColorsController(IVehicleColorService vehicleColorService)
        {
            _vehicleColorService = vehicleColorService;
        }

        // GET: api/vehiclecolors
        [HttpGet]
        public async Task<IActionResult> GetAllColors()
        {
            var colors = await _vehicleColorService.GetVehicleColorsAsync();
            return Ok(colors);
        }

        // GET: api/vehiclecolors/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetColorById(Guid id)
        {
            var color = await _vehicleColorService.GetColorByIdAsync(id);
            if (color == null)
                return NotFound($"Vehicle color with ID {id} not found.");

            return Ok(color);
        }
        
        [HttpGet("bymodel/{modelId}")]
        public async Task<IActionResult> GetColorsByModel(Guid modelId)
        {
            var colors = await _vehicleColorService.GetColorsByModelAsync(modelId);
            if (colors == null || !colors.Any())
                return NotFound($"No colors found for model ID {modelId}.");

            return Ok(colors);
        }

    }
}