using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Required]
        public bool IsSelected { get; set; } = false; // Customer selection

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Store the quoted price at the time of quotation creation

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; } = 1;
        
        // Navigation properties
        public virtual QuotationService QuotationService { get; set; }
        public virtual Part Part { get; set; }
    }
}