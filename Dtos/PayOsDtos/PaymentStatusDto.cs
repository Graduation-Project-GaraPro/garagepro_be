using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BussinessObject;

namespace Dtos.PayOsDtos
{
    public class PaymentStatusDto
    {
        public long OrderCode { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentStatus Status { get; set; } 
        public string? ProviderCode { get; set; } // mã từ PayOS (vd "00")
        public string? ProviderDesc { get; set; }
    }
}
