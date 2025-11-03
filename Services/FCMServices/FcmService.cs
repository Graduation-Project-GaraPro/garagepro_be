using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth.OAuth2;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Services.FCMServices
{
    public class FcmService : IFcmService
    {
        private readonly string _projectId;
        private readonly string _credentialsPath;

        public FcmService(IConfiguration configuration)
        {
            _projectId = configuration["Firebase:ProjectId"]
                ?? throw new ArgumentNullException("Firebase:ProjectId");
            _credentialsPath = configuration["Firebase:CredentialsPath"]
                ?? throw new ArgumentNullException("Firebase:CredentialsPath");
        }

        public async Task SendNotificationAsync(string deviceToken, string title, string body)
        {
            // 1️⃣ Lấy Access Token từ file JSON
            var credential = GoogleCredential.FromFile(_credentialsPath)
                .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

            var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

            // 2️⃣ Tạo payload JSON
            var message = new
            {
                message = new
                {
                    token = deviceToken,
                    notification = new
                    {
                        title = title,
                        body = body
                    }
                }
            };

            var json = JsonSerializer.Serialize(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 3️⃣ Gửi request
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.PostAsync(
                $"https://fcm.googleapis.com/v1/projects/{_projectId}/messages:send",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"FCM Error: {response.StatusCode} - {error}");
            }

            Console.WriteLine("✅ Notification sent successfully.");
        }
    }
}

