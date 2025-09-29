using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class RepairOrderServicePart
    {
        [Key]
        public Guid RepairOrderServicePartId { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey(nameof(RepairOrderService))]
        public Guid RepairOrderServiceId { get; set; }

        [Required]
        [ForeignKey(nameof(Part))]
        public Guid PartId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual RepairOrderService RepairOrderService { get; set; }
        public virtual Part Part { get; set; }
    }
}