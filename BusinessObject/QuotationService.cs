using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Required]
        public bool IsSelected { get; set; } = false; // Customer approval

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Store the quoted price at the time of quotation creation

        // Navigation properties
        public virtual Quotation Quotation { get; set; }
        public virtual Service Service { get; set; }
        // Add the new relationship with QuotationServicePart
        public virtual ICollection<QuotationServicePart> QuotationServiceParts { get; set; } = new List<QuotationServicePart>();
    }
}