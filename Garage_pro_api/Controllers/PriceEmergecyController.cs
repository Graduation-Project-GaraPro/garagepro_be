using Dtos.Emergency;
using Dtos.Emergency.Dtos.Emergency;
using Microsoft.AspNetCore.Mvc;
using Services.EmergencyRequestService;

namespace Garage_pro_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PriceEmergencyController : ControllerBase
    {
        private readonly IPriceService _service;

        public PriceEmergencyController(IPriceService service)
        {
            _service = service;
        }

        [HttpPost("add-price")]
        public async Task<IActionResult> AddPrice([FromBody] PriceEmergencyDto dto)
        {
             await _service.AddPriceAsync(dto);
            return Ok();
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestPrice()
        {
            var price = await _service.GetCurrentPriceAsync();
            return Ok(price);
        }

        [HttpGet("calculate-fee")]
        public async Task<IActionResult> CalculateFee([FromQuery] double distanceKm)
        {
            var fee = await _service.CalculateEmergencyFeeAsync(distanceKm);
            return Ok(new { DistanceKm = distanceKm, Fee = fee });
        }
    }
}
