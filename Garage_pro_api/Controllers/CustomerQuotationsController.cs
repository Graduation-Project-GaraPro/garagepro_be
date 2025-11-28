using System.Security.Claims;
using AutoMapper;
using BusinessObject;
using Dtos.Quotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.QuotationServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerQuotationsController : ControllerBase
    {
        private readonly ICustomerResponseQuotationService _customerResponseService;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerQuotationsController> _logger;

        public CustomerQuotationsController(
            ICustomerResponseQuotationService customerResponseService,
            IMapper mapper,
            ILogger<CustomerQuotationsController> logger)
        {
            _customerResponseService = customerResponseService;
            _mapper = mapper;
            _logger = logger;
        }

        [Authorize]
        [HttpPut("customer-response")]
        public async Task<ActionResult<QuotationDto>> ProcessCustomerResponse([FromBody] CustomerQuotationResponseDto responseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var updatedQuotation = await _customerResponseService.ProcessCustomerResponseAsync(responseDto,userId);
                return Ok(updatedQuotation);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
