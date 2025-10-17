using Dtos.Vehicles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.VehicleServices;

namespace Garage_pro_api.Controllers.Vehicle
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class VehicleBrandsController : ControllerBase
    {
        private readonly IVehicleBrandServices _vehicleBrandService;

        public VehicleBrandsController(IVehicleBrandServices vehicleBrandService)
        {
            _vehicleBrandService = vehicleBrandService;
        }

        // GET: api/vehiclebrands
        [HttpGet]
        public async Task<IActionResult> GetAllBrands()
        {
            var brands = await _vehicleBrandService.GetAllBrandsAsync();
            return Ok(brands);
        }

        // GET: api/vehiclebrands/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBrandById(Guid id)
        {
            var brand = await _vehicleBrandService.GetBrandByIdAsync(id);
            if (brand == null)
                return NotFound($"Brand with ID {id} not found.");

            return Ok(brand);
        }

        // POST: api/vehiclebrands
        [HttpPost]
        [Authorize(Roles = "Admin")] // Chỉ admin có quyền tạo brand
        public async Task<IActionResult> CreateBrand(CreateVehicleBrandDto brandDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdBrand = await _vehicleBrandService.CreateBrandAsync(brandDto);
            return CreatedAtAction(nameof(GetBrandById), new { id = createdBrand.BrandID }, createdBrand);
        }
    }
}