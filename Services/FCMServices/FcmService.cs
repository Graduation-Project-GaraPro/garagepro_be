using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using BusinessObject.FcmDataModels;
using BusinessObject.Enums;

namespace Services.FCMServices
{
    public class FcmService : IFcmService
    {
        private readonly string _projectId;
        private readonly string _serviceAccountJson;

        public FcmService(IConfiguration configuration)
        {
            _projectId = configuration["Firebase:ProjectId"]
                ?? throw new ArgumentNullException("Firebase:ProjectId");

            // Lấy section Firebase:ServiceAccount (object) và convert thành JSON string
            var serviceAccountSection = configuration.GetSection("Firebase:ServiceAccount");
            if (!serviceAccountSection.Exists())
            {
                throw new ArgumentNullException("Firebase:ServiceAccount",
                    "Firebase:ServiceAccount section is missing in configuration.");
            }

            // Convert các key-value con thành dictionary => serialize ra JSON
            var dict = serviceAccountSection.GetChildren()
                                            .ToDictionary(c => c.Key, c => c.Value);

            _serviceAccountJson = JsonSerializer.Serialize(dict);

            Console.WriteLine("[FCM] Loaded ServiceAccount JSON from configuration:");
            Console.WriteLine(_serviceAccountJson);
        }

        private GoogleCredential CreateCredentialFromConfig()
        {
            try
            {
                Console.WriteLine("[FCM] Creating GoogleCredential from appsettings JSON...");

                var credential = GoogleCredential
                    .FromJson(_serviceAccountJson)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

                Console.WriteLine("[FCM] GoogleCredential created.");
                return credential;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FCM] Failed to create GoogleCredential from JSON:");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task SendFcmMessageAsync(string deviceToken, FcmDataPayload payload)
        {
            if (string.IsNullOrEmpty(deviceToken))
                throw new ArgumentNullException(nameof(deviceToken));

            var credential = CreateCredentialFromConfig();

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
                    data = payload.ToDictionary()
                }
            };

            var json = JsonSerializer.Serialize(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

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

            Console.WriteLine("Notification sent successfully.");
        }

        public async Task SendFcmMessageWithDataAsync(string deviceToken, FcmDataPayload payload)
        {
            if (string.IsNullOrEmpty(deviceToken))
                throw new ArgumentNullException(nameof(deviceToken));

            var credential = CreateCredentialFromConfig();

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

            object messageBody;

            if (payload.Type == NotificationType.Emergency)
            {
                var data = payload.ToDictionary();

                data["type"] = "Emergency";
                if (!data.ContainsKey("title")) data["title"] = payload.Title ?? "Emergency case";
                if (!data.ContainsKey("body")) data["body"] = payload.Body ?? "New Emergency case for you";
                if (!data.ContainsKey("screen")) data["screen"] = "ReportsFragment";

                messageBody = new
                {
                    message = new
                    {
                        token = deviceToken,
                        data = data,
                        android = new
                        {
                            priority = "HIGH"
                        }
                    }
                };
            }
            else
            {
                messageBody = new
                {
                    message = new
                    {
                        token = deviceToken,
                        notification = new
                        {
                            title = payload.Title ?? "Notification",
                            body = payload.Body ?? ""
                        },
                        data = payload.ToDictionary()
                    }
                };
            }

            var json = JsonSerializer.Serialize(messageBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

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

            Console.WriteLine("Notification sent successfully.");
        }
    }
}
