using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Customer
    {
        [Key]
        public Guid CustomerId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BranchId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        // Navigation properties
        public virtual Branch Branch { get; set; }
        public virtual ICollection<RepairOrder> RepairOrders { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Vehicle> Vehicles { get; set; }
    }
}