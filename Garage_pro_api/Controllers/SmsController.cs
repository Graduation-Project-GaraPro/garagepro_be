using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        
       
            private readonly HttpClient _httpClient;
            private readonly string _apiKey;

            public SmsController(IConfiguration config, IHttpClientFactory httpClientFactory)
            {
                _httpClient = httpClientFactory.CreateClient();
                _apiKey = config["SpeedSMS:ApiKey"] ?? throw new ArgumentNullException("SpeedSMS:ApiKey");
            }

            // GET /api/sms/userinfo
            [HttpGet("userinfo")]
            public async Task<IActionResult> GetUserInfo()
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.speedsms.vn/index.php/user/info");
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_apiKey}:"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                return Content(content, "application/json");
            }

            // POST /api/sms/send
            [HttpPost("send")]
            public async Task<IActionResult> SendSms([FromBody] SmsRequest model)
            {
                var url = "https://api.speedsms.vn/index.php/sms/send";

                var payload = new Dictionary<string, object>
                {
                    ["to"] = new[] { model.Phone },
                    ["content"] = model.Content,
                    ["sms_type"] = model.Type
                };

                if (!string.IsNullOrEmpty(model.Sender))
                {
                    payload["sender"] = model.Sender;
                }

                var json = JsonSerializer.Serialize(payload);

                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_apiKey}:"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                return Content(responseBody, "application/json");
            }
        }

        // Request model cho sendSMS
        public class SmsRequest
        {
            public string Phone { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public int Type { get; set; } = 2; // default CSKH
            public string? Sender { get; set; }
        }
    
}
