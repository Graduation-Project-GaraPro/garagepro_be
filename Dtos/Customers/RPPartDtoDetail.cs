using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Customers
{
    public class RPPartDtoDetail
    {
        public Guid PartId { get; set; }
        public String PartName { get; set; }
        public int Quantity { get; set; }
        public decimal price { get; set; }
    }
}
