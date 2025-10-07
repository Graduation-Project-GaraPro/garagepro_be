using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Customers
{
    public class RequestService
    {
        public Guid RequestServiceId { get; set; }
        public Guid RepairRequestId { get; set; }
        public Guid ServiceId { get; set; }
        public int numberService { get; set; }//number of service
        public long TotalAmount { get; set; }

        // Navigation properties
        public virtual RepairRequest RepairRequest { get; set; }
        public virtual Service Service { get; set; }
    }
}
