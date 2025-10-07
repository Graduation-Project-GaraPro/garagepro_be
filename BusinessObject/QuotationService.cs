using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class QuotationService
    {
        [Key]
        public Guid QuotationServiceId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid QuotationId { get; set; }

        [Required]
        public Guid ServiceId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ServicePrice { get; set; }

        public int Quantity { get; set; } = 1;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Customer requested alternative parts (for display only, not system updates)
        [MaxLength(1000)]
        public string CustomerRequestedParts { get; set; } // JSON or text description of requested parts

        // Navigation properties
        public virtual Quotation Quotation { get; set; }
        public virtual Service Service { get; set; }
        public virtual ICollection<QuotationServicePart> QuotationServiceParts { get; set; } = new List<QuotationServicePart>();
    }
}