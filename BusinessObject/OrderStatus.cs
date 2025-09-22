using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class OrderStatus
    {
        [Key]
        public Guid OrderStatusId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string StatusName { get; set; }

        // Navigation property
        public virtual ICollection<RepairOrder> RepairOrders { get; set; }
        public virtual ICollection<Label> Labels { get; set; }
    }
}