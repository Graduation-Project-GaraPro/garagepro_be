using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Branches
{
    public class ServicePart
    {
        [Key]
        public Guid ServicePartId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServiceId { get; set; }

        [Required]
        public Guid PartId { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Service Service { get; set; }
        public virtual Part Part { get; set; }
    }
}