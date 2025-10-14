using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Authentication;

namespace BusinessObject
{
    public class Quotation
    {
        [Key]
        public Guid QuotationId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid InspectionId { get; set; }

        [Required]
        public string UserId { get; set; } // Customer ID

        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SentToCustomerAt { get; set; }

        public DateTime? CustomerResponseAt { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } // Pending, Sent, Approved, Rejected, Expired

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [MaxLength(500)]
        public string Note { get; set; }

        public DateTime? ExpiresAt { get; set; }

        // Navigation properties
        public virtual Inspection Inspection { get; set; }
        public virtual ApplicationUser User { get; set; } // Customer
        public virtual Vehicle Vehicle { get; set; }
        public virtual ICollection<QuotationService> QuotationServices { get; set; } = new List<QuotationService>();
        public virtual ICollection<QuotationPart> QuotationParts { get; set; } = new List<QuotationPart>();
    }
}