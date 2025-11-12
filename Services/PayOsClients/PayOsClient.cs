using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessObject.PayOsModels;
using Microsoft.Extensions.Options;

namespace Services.PayOsClients
{
    public class PayOsClient : IPayOsClient
    {
        private readonly HttpClient _http;
        private readonly PayOsOptions _opt;
        private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

        public PayOsClient(HttpClient http, IOptions<PayOsOptions> opt)
        {
            _http = http;
            _opt = opt.Value;
            _http.BaseAddress = new Uri(_opt.BaseUrl);
            _http.DefaultRequestHeaders.Add("x-client-id", _opt.ClientId);
            _http.DefaultRequestHeaders.Add("x-api-key", _opt.ApiKey);
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private string HmacSha256(string key, string data)
        {
            using var h = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = h.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public async Task<PayOsResponse<CreatePaymentLinkResponse>> CreatePaymentLinkAsync(CreatePaymentLinkRequest req, CancellationToken ct = default)
        {
            // Chuỗi ký theo docs (alphabetical)
            try
            {
                var dataToSign =
                $"amount={req.amount}&cancelUrl={req.cancelUrl}&description={req.description}&orderCode={req.orderCode}&returnUrl={req.returnUrl}";
                var signature = HmacSha256(_opt.ChecksumKey, dataToSign);

                var payload = req with { signature = signature };

                using var content = new StringContent(JsonSerializer.Serialize(payload, s_json), Encoding.UTF8, "application/json");
                //using var res = await _http.PostAsync("/v2/payment-requests", content, ct);
                using var res = await _http.PostAsync("/v2/payment-requests", content);
                var body = await res.Content.ReadAsStringAsync(ct);

                if (!res.IsSuccessStatusCode)
                    throw new HttpRequestException($"payOS error {res.StatusCode}: {body}");

                return JsonSerializer.Deserialize<PayOsResponse<CreatePaymentLinkResponse>>(body, s_json)!;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<PayOsResponse<CancelPaymentLinkData>> CancelPaymentLinkAsync(
            long orderCode,
            string? cancelReason = null,
            CancellationToken ct = default)
        {
            // 1) Chuẩn bị payload + signature (alphabetical by key)
            var reason = cancelReason ?? "cancelled_by_merchant";
            // Ký: cancelReason & orderCode (đảm bảo thứ tự key a→z)
            var dataToSign = $"cancelReason={reason}&orderCode={orderCode}";
            var signature = HmacSha256(_opt.ChecksumKey, dataToSign);

            var payload = new CancelPaymentLinkRequest(
                cancelReason: reason,
                signature: signature
            );

            // 2) Gọi API PayOS
            using var content = new StringContent(JsonSerializer.Serialize(payload, s_json), Encoding.UTF8, "application/json");
            using var res = await _http.PostAsync($"/v2/payment-requests/{orderCode}/cancel", content, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"payOS cancel error {res.StatusCode}: {body}");

            // 3) Parse response
            var parsed = JsonSerializer.Deserialize<PayOsResponse<CancelPaymentLinkData>>(body, s_json);
            if (parsed is null)
                throw new InvalidOperationException("Unable to parse PayOS cancel response.");

            return parsed;
        }




        // (Tuỳ chọn) Xác minh webhook: rawBody là JSON gốc, signature lấy từ trường "signature" hoặc header theo đặc tả webhook
        public bool VerifyWebhookSignature(JsonElement dataEl, string signature)
        {
            var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var p in dataEl.EnumerateObject())
                dict[p.Name] = p.Value;

            string Flatten(object? v)
            {
                if (v is null) return "";
                if (v is JsonElement je)
                {
                    return je.ValueKind switch
                    {
                        JsonValueKind.Null or JsonValueKind.Undefined => "",
                        JsonValueKind.Array or JsonValueKind.Object => JsonSerializer.Serialize(je),
                        _ => je.ToString() ?? ""
                    };
                }
                return v.ToString() ?? "";
            }

            var pairs = dict.OrderBy(kv => kv.Key, StringComparer.Ordinal)
                            .Select(kv => $"{kv.Key}={Flatten(kv.Value)}");
            var canonical = string.Join("&", pairs);

            using var h = new System.Security.Cryptography.HMACSHA256(
                System.Text.Encoding.UTF8.GetBytes(_opt.ChecksumKey)
            );
            var calc = Convert.ToHexString(h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(canonical)))
                        .ToLowerInvariant();

            return string.Equals(calc, signature, StringComparison.OrdinalIgnoreCase);
        }
    }
}
