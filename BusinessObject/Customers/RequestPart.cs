using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Customers
{
    public class RequestPart
    {
        public Guid RequestPartId { get; set; }
        public Guid RepairRequestID { get; set; }
        public Guid PartId { get; set; }
        public int number { get; set; }
        public long totalAmount { get; set; }

        public virtual RepairRequest RepairRequest { get; set; }
        public virtual Part Part { get; set; }
    }
}
