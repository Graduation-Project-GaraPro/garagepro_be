using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessObject.PayOsModels;

namespace Services.PayOsClients
{
    public interface IPayOsClient
    {
        Task<PayOsResponse<CreatePaymentLinkResponse>> CreatePaymentLinkAsync(CreatePaymentLinkRequest req, CancellationToken ct = default);
        Task<PayOsResponse<CancelPaymentLinkData>> CancelPaymentLinkAsync(
            long orderCode,
            string? cancelReason = null,
            CancellationToken ct = default);

        bool VerifyWebhookSignature(JsonElement dataEl , string signature); 
    }
}
