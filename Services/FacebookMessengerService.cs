
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Services
{
    public interface IFacebookMessengerService
    {
        Task<FacebookMessageResponse> SendMessageAsync(string messageText);
    }

    public class FacebookMessageResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public string Error { get; set; }
    }

    public class FacebookMessengerService : IFacebookMessengerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _pageAccessToken;
        private const string RECIPIENT_ID = "32104322482544720"; // Fix cứng
        private const string MESSAGING_TYPE = "MESSAGE_TAG"; // Fix cứng
        private const string MESSAGING_TAG = "ACCOUNT_UPDATE"; // Fix cứng

        public FacebookMessengerService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _pageAccessToken = "EAAQH8MTNnYIBP0n4IKfLEZCESG30ld6HSCjsj2c0sjBObM1upBUqjlSQkvzw1gx5XlTL8icl6YtLZBgECwe0D2Tl9A3tBAZBJbZBZADZCqYpMPl6KzTOYLQAYZBtiJZA5ZC36HSczDEKy4MuaZB3yrTZAjowmIwXOTnrx7DBYv7MnrexzN7wR3AC2MfLH0Fu8QMyKhKTEVddTsy6RGezS8ehS4GQAZDZD";
        }

        public async Task<FacebookMessageResponse> SendMessageAsync(string messageText)
        {
            try
            {
                if (string.IsNullOrEmpty(_pageAccessToken))
                {
                    return new FacebookMessageResponse
                    {
                        Success = false,
                        Message = "Page Access Token không được cấu hình"
                    };
                }

                var client = _httpClientFactory.CreateClient();
                var apiUrl = $"https://graph.facebook.com/v24.0/897221366798180/messages?access_token={_pageAccessToken}";

                // Tạo request body với recipient và messaging_type fix cứng
                var requestBody = new
                {
                    recipient = new { id = RECIPIENT_ID },
                    messaging_type = MESSAGING_TYPE,
                    tag = MESSAGING_TAG,
                    message = new { text = messageText }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new FacebookMessageResponse
                    {
                        Success = true,
                        Message = "Tin nhắn đã được gửi thành công",
                        Data = JsonSerializer.Deserialize<object>(responseContent)
                    };
                }
                else
                {
                    return new FacebookMessageResponse
                    {
                        Success = false,
                        Message = "Lỗi khi gửi tin nhắn",
                        Error = responseContent
                    };
                }
            }
            catch (Exception ex)
            {
                return new FacebookMessageResponse
                {
                    Success = false,
                    Message = "Lỗi server",
                    Error = ex.Message
                };
            }
        }
    }
}
