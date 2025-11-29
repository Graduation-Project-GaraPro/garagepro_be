using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Label
    {
        [Key]
        public Guid LabelId { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey(nameof(OrderStatus))]
        public int OrderStatusId { get; set; } // Changed from Guid to int

        [Required]
        [MaxLength(100)]
        public string LabelName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)] // Color name
        public string ColorName { get; set; }

        [Required]
        [MaxLength(7)] // Hex color code like #FF5733
        public string HexCode { get; set; }

        // Default label for this status (only one per status should be true)
        public bool IsDefault { get; set; } = false;

        // Navigation properties
        public virtual OrderStatus OrderStatus { get; set; } = null!;
        
        // Many-to-many relationship with RepairOrders
        public virtual ICollection<RepairOrder> RepairOrders { get; set; } = new List<RepairOrder>();
    }
}