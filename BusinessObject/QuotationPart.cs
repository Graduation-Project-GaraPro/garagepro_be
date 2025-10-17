using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    public class QuotationPart
    {
        [Key]
        public Guid QuotationPartId { get; set; } = Guid.NewGuid();

        //[Required]
        // public Guid QuotationId { get; set; }
        [Required]
        public Guid QuotationServiceId { get; set; }

        [Required]
        public Guid PartId { get; set; }

        [Required]
        public bool IsSelected { get; set; } = false; // Customer approval

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
       // public virtual Quotation Quotation { get; set; }
        public virtual QuotationService QuotationService { get; set; }
        public virtual Part Part { get; set; }
    }
}