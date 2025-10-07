using BusinessObject.Branches; // Add this using statement
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class QuotationServicePart
    {
        [Key]
        public Guid QuotationServicePartId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid QuotationServiceId { get; set; }

        [Required]
        public Guid PartId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual QuotationService QuotationService { get; set; }
        public virtual Part Part { get; set; }
    }
}