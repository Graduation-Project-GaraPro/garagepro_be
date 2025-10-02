using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class RepairOrderService
    {
        [Key]
        public Guid RepairOrderServiceId { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey(nameof(RepairOrder))]
        public Guid RepairOrderId { get; set; }

        [Required]
        [ForeignKey(nameof(Service))]
        public Guid ServiceId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ServicePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualDuration { get; set; } // in hours

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual RepairOrder RepairOrder { get; set; }
        public virtual Service Service { get; set; }
        public virtual ICollection<RepairOrderServicePart> RepairOrderServiceParts { get; set; } = new List<RepairOrderServicePart>();
    }
}