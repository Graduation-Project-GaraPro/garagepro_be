using BusinessObject.Campaigns;
using Dtos.Campaigns;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.CampaignServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerPromotionalsController : ControllerBase
    {
        private readonly IPromotionalCampaignService _service;

        public CustomerPromotionalsController(IPromotionalCampaignService service)
        {
            _service = service;
        }

        [HttpGet("services/{serviceId}/customer-promotions")]
        public async Task<ActionResult<CustomerPromotionResponse>> GetCustomerPromotionsForService(
    Guid serviceId, [FromQuery] decimal currentOrderValue = 0)
        {
            try
            {
                var response = await _service.GetCustomerPromotionsForServiceAsync(serviceId, currentOrderValue);
                return Ok(response);
            }
            
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
