using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Authentication;
using BusinessObject.Enums; // Add this using statement
using BusinessObject.Customers;

namespace BusinessObject
{
    public class Quotation
    {
        [Key]
        public Guid QuotationId { get; set; } = Guid.NewGuid();

        // Made InspectionId nullable
        public Guid? InspectionId { get; set; }
        
        // Add nullable relationship to RepairOrder
        public Guid? RepairOrderId { get; set; }

        [Required]
        public string UserId { get; set; } // Customer ID

        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? SentToCustomerAt { get; set; }

        public DateTime? CustomerResponseAt { get; set; }

        // Change the Status property to use the enum
        public QuotationStatus Status { get; set; } // Pending, Sent, Approved, Rejected, Expired

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal InspectionFee { get; set; } = 0; // Fee for inspection service

        [MaxLength(500)]
        public string? Note { get; set; }
        public string? CustomerNote { get; set; }

        public DateTime? ExpiresAt { get; set; }

        // Navigation properties
        public virtual Inspection Inspection { get; set; }
        public virtual RepairOrder RepairOrder { get; set; }
        public virtual ApplicationUser User { get; set; } // Customer
        public virtual Vehicle Vehicle { get; set; }
        public virtual ICollection<QuotationService> QuotationServices { get; set; } = new List<QuotationService>();
    }
}