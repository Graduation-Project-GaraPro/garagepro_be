using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.PayOsModels
{
    public record CancelPaymentLinkData(
    long orderCode,
    string status,              // "CANCELLED" nếu thành công
    string? message
);
}
