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
        public bool IsSelected { get; set; } = false; 

        public bool IsRequired { get; set; } = false; 

        public bool IsGood { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } 
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