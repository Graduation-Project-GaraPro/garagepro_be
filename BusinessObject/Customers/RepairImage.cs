using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Customers
{
    public class RepairImage
    {
        [Key]
        public Guid ImageId { get; set; }

        [Required]
        public Guid RepairRequestId { get; set; }

        [ForeignKey("RepairRequestId")]
        public virtual RepairRequest RepairRequest { get; set; }

        [Required]
        [StringLength(500)] 
        public string ImageUrl { get; set; }
    }
}
