using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Customers
{
    public class RequestServiceDto
    {
        public Guid ServiceID { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
    }
}
