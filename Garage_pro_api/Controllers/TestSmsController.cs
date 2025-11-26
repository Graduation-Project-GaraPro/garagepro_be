using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestSmsController : ControllerBase
    {
        [HttpGet("send")]
        public async Task<IActionResult> SendSms(string phone, string content)
        {
            string apiKey = "Awv6gMSfpuKsENBfzlrCpkQTVmmTQ1AC";  // <-- THAY Ở ĐÂY

            // Basic Auth = base64("apiKey:")
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey + ":"));

            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", auth);

            var payload = new
            {
                to = new[] { phone },
                content = content,
                type = 5,          // gửi SMS thường (không brandname)
                brandname = "5509e331626b422f"     // để trống
            };

            var body = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await http.PostAsync(
                "https://api.speedsms.vn/index.php/sms/send",
                body
            );

            string responseText = await response.Content.ReadAsStringAsync();

            return Ok(new
            {
                status = response.StatusCode.ToString(),
                response = responseText
            });
        }
    }
}
