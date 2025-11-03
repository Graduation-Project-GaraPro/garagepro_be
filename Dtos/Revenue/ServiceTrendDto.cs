using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Revenue
{
    public class ServiceTrendDto
    {
        public string Period { get; set; } = string.Empty;

        // Sử dụng dictionary để lưu revenue theo service name
        public Dictionary<string, decimal> Services { get; set; } = new();
    }

}
