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
        [Key]
        public Guid RequestServiceId { get; set; } = Guid.NewGuid();
        [Required]
        public Guid RepairRequestId { get; set; }
        [Required]
        public Guid ServiceId { get; set; }
      

        public decimal ServiceFee { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RepairRequestId))]
        public virtual RepairRequest RepairRequest { get; set; }
        [ForeignKey(nameof(ServiceId))]
        public virtual Service Service { get; set; }
        public virtual ICollection<RequestPart> RequestParts { get; set; } = new List<RequestPart>();
    }
}
