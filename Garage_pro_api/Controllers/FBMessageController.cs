using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Services;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FBMessageController : ControllerBase
    {
        private readonly IFacebookMessengerService _facebookMessengerService;

        public FBMessageController(IFacebookMessengerService facebookMessengerService)
        {
            _facebookMessengerService = facebookMessengerService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var result = await _facebookMessengerService.SendMessageAsync(request.Message);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }

    public class SendMessageRequest
    {
        public string Message { get; set; }
    }
}
