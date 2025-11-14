using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.PayOsModels
{
    public record CancelPaymentLinkRequest(
    string? cancelReason,
    string? signature = null
    );
}
