using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Campaigns;

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

        public bool IsRequired { get; set; } = false; // Indicates if this is a required service

        public bool IsGood { get; set; } = false; // Service condition is Good - view only, no payment needed

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Store the quoted price at the time of quotation creation
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; } = 0;


        public decimal FinalPrice { get; set; } = 0;


        public Guid? AppliedPromotionId { get; set; }
        
        public virtual PromotionalCampaign? AppliedPromotion { get; set; }

        // Navigation properties
        public virtual Quotation Quotation { get; set; }
        public virtual Service? Service { get; set; }
        // Add the new relationship with QuotationServicePart
        public virtual ICollection<QuotationServicePart> QuotationServiceParts { get; set; } = new List<QuotationServicePart>();
    }
}