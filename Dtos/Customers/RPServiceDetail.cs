using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Customers
{
    public class RPServiceDetail
    {
        public Guid ServiceId { get; set; }
        public List<RPPartDtoDetail> Parts { get; set; } = new List<RPPartDtoDetail>();
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        //public int quatity { get; set; }
    }
}
