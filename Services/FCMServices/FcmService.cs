using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;       // <-- nhớ using
using BusinessObject.FcmDataModels;
using BusinessObject.Enums;

namespace Services.FCMServices
{
    public class FcmService : IFcmService
    {
        private readonly string _projectId;
        private readonly string _serviceAccountJson;
        private readonly ILogger<FcmService> _logger;

        public FcmService(IConfiguration configuration, ILogger<FcmService> logger)
        {
            _logger = logger;
            _logger.LogInformation("[FCM] FcmService constructor starting...");

            // Log thử environment (nếu có)
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                      ?? "Unknown";
            _logger.LogInformation("[FCM] ASPNETCORE_ENVIRONMENT = {Env}", env);

            // 1. Lấy ProjectId
            _projectId = configuration["Firebase:ProjectId"];
            if (string.IsNullOrEmpty(_projectId))
            {
                _logger.LogError("[FCM] Firebase:ProjectId is NULL or empty in configuration!");
                throw new ArgumentNullException("Firebase:ProjectId");
            }
            _logger.LogInformation("[FCM] Firebase:ProjectId = {ProjectId}", _projectId);

            // 2. Lấy section ServiceAccount
            var serviceAccountSection = configuration.GetSection("Firebase:ServiceAccount");
            if (!serviceAccountSection.Exists())
            {
                _logger.LogError("[FCM] Firebase:ServiceAccount section is MISSING in configuration.");
                throw new ArgumentNullException("Firebase:ServiceAccount",
                    "Firebase:ServiceAccount section is missing in configuration.");
            }

            var children = serviceAccountSection.GetChildren().ToList();
            _logger.LogInformation("[FCM] Firebase:ServiceAccount has {Count} child keys.", children.Count);
            foreach (var child in children)
            {
                // Không log value để tránh lộ secret
                _logger.LogInformation("[FCM] ServiceAccount key found: {Key} (hasValue={HasValue})",
                    child.Key,
                    !string.IsNullOrEmpty(child.Value));
            }

            // 3. Convert sang JSON
            var dict = children.ToDictionary(c => c.Key, c => c.Value);

            // Cẩn thận: KHÔNG log full JSON vì chứa private_key
            _serviceAccountJson = JsonSerializer.Serialize(dict);

            _logger.LogInformation("[FCM] ServiceAccount JSON serialized successfully. Length = {Length}",
                _serviceAccountJson.Length);
        }

        private GoogleCredential CreateCredentialFromConfig()
        {
            try
            {
                _logger.LogInformation("[FCM] Creating GoogleCredential from appsettings JSON...");

                var credential = GoogleCredential
                    .FromJson(_serviceAccountJson)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

                _logger.LogInformation("[FCM] GoogleCredential created successfully.");
                return credential;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FCM] Failed to create GoogleCredential from JSON.");
                throw;
            }
        }

        public async Task SendFcmMessageAsync(string deviceToken, FcmDataPayload payload)
        {
            if (string.IsNullOrEmpty(deviceToken))
                throw new ArgumentNullException(nameof(deviceToken));

            _logger.LogInformation("[FCM] SendFcmMessageAsync started. Token prefix: {TokenPrefix}",
                deviceToken.Length > 10 ? deviceToken.Substring(0, 10) : deviceToken);

            var credential = CreateCredentialFromConfig();

            string accessToken;
            try
            {
                _logger.LogInformation("[FCM] Requesting access token...");
                accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
                _logger.LogInformation("[FCM] Access token acquired. Length = {Length}", accessToken?.Length ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FCM] Failed to get access token.");
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
            _logger.LogInformation("[FCM] Request body JSON length = {Length}", json.Length);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = $"https://fcm.googleapis.com/v1/projects/{_projectId}/messages:send";
            _logger.LogInformation("[FCM] Sending POST to {Url}", url);

            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("[FCM] Error response from FCM: {StatusCode} - {Error}",
                    response.StatusCode, error);

                throw new Exception($"FCM Error: {response.StatusCode} - {error}");
            }

            _logger.LogInformation("[FCM] Notification sent successfully.");
        }

        public async Task SendFcmMessageWithDataAsync(string deviceToken, FcmDataPayload payload)
        {
            if (string.IsNullOrEmpty(deviceToken))
                throw new ArgumentNullException(nameof(deviceToken));

            _logger.LogInformation("[FCM] SendFcmMessageWithDataAsync started. Type = {Type}", payload.Type);

            var credential = CreateCredentialFromConfig();

            string accessToken;
            try
            {
                _logger.LogInformation("[FCM] Requesting access token...");
                accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
                _logger.LogInformation("[FCM] Access token acquired. Length = {Length}", accessToken?.Length ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FCM] Failed to get access token.");
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
            _logger.LogInformation("[FCM] Request body JSON length = {Length}", json.Length);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = $"https://fcm.googleapis.com/v1/projects/{_projectId}/messages:send";
            _logger.LogInformation("[FCM] Sending POST to {Url}", url);

            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("[FCM] Error response from FCM: {StatusCode} - {Error}",
                    response.StatusCode, error);

                throw new Exception($"FCM Error: {response.StatusCode} - {error}");
            }

            _logger.LogInformation("[FCM] Notification sent successfully.");
        }
    }
}
