using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.PayOsModels
{
      public record CreatePaymentLinkRequest(
        long orderCode,
        int amount,
        string description,
        string cancelUrl,
        string returnUrl,
        string? buyerName = null,
        string? buyerEmail = null,
        object? invoice = null,
        long? expiredAt = null,
        string? signature = null
    );
}
