using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace Services.SmsSenders
{
    public class SpeedSmsClient
    {
        private readonly HttpClient _client;

        public SpeedSmsClient(string apiKey)
        {
            _client = new HttpClient();
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey + ":"));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        }

        public async Task<string> SendSmsAsync(string phone, string message, int type = 2)
        {
            // type:
            // 1: Brandname QC
            // 2: Brandname CSKH
            // 3: Brandname OTP
            // 5: SMS thường (không brandname)

            var payload = new
            {
                to = new[] { phone },
                content = message,
                type = type,
                brandname = "" // để trống nếu dùng SMS thường
            };

            var response = await _client.PostAsync(
                "https://api.speedsms.vn/index.php/sms/send",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            );

            return await response.Content.ReadAsStringAsync();
        }
    }
}
