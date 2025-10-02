using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Services.SmsSenders
{
    public class SpeedSmsSender : ISmsSender
    {
        private readonly ILogger<SpeedSmsSender> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string? _sender;
        private readonly int _smsType;

        public SpeedSmsSender(
            ILogger<SpeedSmsSender> logger,
            IConfiguration config,
            HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;

            _apiKey = config["SpeedSMS:ApiKey"] ?? throw new ArgumentNullException("SpeedSMS:ApiKey");
            _sender = config["SpeedSMS:Sender"];   // có thể null hoặc brandname
            _smsType = int.TryParse(config["SpeedSMS:Type"], out var t) ? t : 2; // mặc định dùng CSKH
        }

        public async Task SendSmsAsync(string number, string message)
        {
            var url = "https://api.speedsms.vn/index.php/sms/send";

            // payload cơ bản
            var payload = new Dictionary<string, object>
            {
                ["to"] = new[] { number },
                ["content"] = message,
                ["sms_type"] = _smsType
            };

            // chỉ thêm sender nếu có cấu hình
            if (!string.IsNullOrEmpty(_sender))
            {
                payload["sender"] = _sender;
            }

            var json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_apiKey}:"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            // check status trong JSON chứ không chỉ HTTP status
            if (!response.IsSuccessStatusCode || responseBody.Contains("\"status\":\"error\""))
            {
                _logger.LogError("SpeedSMS send failed: {StatusCode}, {Response}", response.StatusCode, responseBody);
                throw new Exception($"SpeedSMS send failed: {response.StatusCode}, {responseBody}");
            }

            _logger.LogInformation("SpeedSMS sent to {Number}, message={Message}, response={Response}",
                number, message, responseBody);
        }
    }
}
