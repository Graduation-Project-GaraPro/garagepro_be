using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.VehicleServices;

namespace Garage_pro_api.Controllers.Vehicle
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Chỉ user đăng nhập mới được truy cập, có thể remove nếu muốn public
    public class VehicleModelsController : ControllerBase
    {
        private readonly IVehicleModelService _vehicleModelService;

        public VehicleModelsController(IVehicleModelService vehicleModelService)
        {
            _vehicleModelService = vehicleModelService;
        }

        // GET: api/vehiclemodels
        [HttpGet]
        public async Task<IActionResult> GetAllModels()
        {
            var models = await _vehicleModelService.GetAllVehicleModelsAsync();
            return Ok(models);
        }

        // GET: api/vehiclemodels/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetModelById(Guid id)
        {
            var model = await _vehicleModelService.GetVehicleModelByIdAsync(id);
            if (model == null)
                return NotFound($"Vehicle model with ID {id} not found.");

            return Ok(model);
        }
        // GET: api/vehiclemodels/bybrand/{brandId}
        [HttpGet("bybrand/{brandId}")]
        public async Task<IActionResult> GetModelsByBrand(Guid brandId)
        {
            var models = await _vehicleModelService.GetModelsByBrandAsync(brandId);
            if (models == null || !models.Any())
                return NotFound($"No models found for brand ID {brandId}.");

            return Ok(models);
        }
    }
}