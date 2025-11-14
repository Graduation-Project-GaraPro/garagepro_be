using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.PayOsModels
{
    public class PayOsOptions
    {
        public string ClientId { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public string ChecksumKey { get; set; } = default!;
        public string BaseUrl { get; set; } = "https://api-merchant.payos.vn";
    }
}
