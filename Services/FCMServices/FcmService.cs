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
using BusinessObject.FcmDataModels;

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
            _credentialsPath = Path.Combine(AppContext.BaseDirectory, "Keys", "garapro-firebase-firebase-adminsdk-fbsvc-292a41367b.json");
        }

      
        public async Task SendFcmMessageAsync(string deviceToken, FcmDataPayload payload)
        {
            if (string.IsNullOrEmpty(deviceToken))
                throw new ArgumentNullException(nameof(deviceToken));

            Console.WriteLine("[FCM] Using credentials at: " + _credentialsPath);

            if (!File.Exists(_credentialsPath))
                throw new FileNotFoundException("Credentials file not found", _credentialsPath);

            GoogleCredential credential;

            try
            {
                using var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read);
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
                Console.WriteLine("[FCM] GoogleCredential created.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FCM] Failed to create GoogleCredential:");
                Console.WriteLine(ex.ToString());
                throw;
            }

            string accessToken;
            try
            {
                Console.WriteLine("[FCM] Requesting access token...");
                accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
                Console.WriteLine("[FCM] Access token acquired.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FCM] Failed to get access token:");
                Console.WriteLine(ex.ToString());
                throw;
            }

            // 2️⃣ Tạo payload JSON (notification + data)
            var message = new
            {
                message = new
                {
                    token = deviceToken,
                    notification = new
                    {
                        title = payload.Title ?? "Notification",
                        body = payload.Body ?? ""
                    },
                    data = payload.ToDictionary() // tự động map từ FcmDataPayload
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

            Console.WriteLine(" Notification sent successfully.");
        }
    }
}

