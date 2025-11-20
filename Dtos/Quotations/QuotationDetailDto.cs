using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dtos.Campaigns;

namespace Dtos.Quotations
{
    public class QuotationDetailDto
    {
        public Guid QuotationId { get; set; }
        public Guid InspectionId { get; set; }
        public Guid? RepairOrderId { get; set; }
        public string UserId { get; set; }
        public Guid VehicleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentToCustomerAt { get; set; }
        public DateTime? CustomerResponseAt { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? Note { get; set; }
        public string? CustomerNote { get; set; }
        public DateTime? ExpiresAt { get; set; }

        // Navigation properties
        public string CustomerName { get; set; }
        public string VehicleInfo { get; set; }
        public ICollection<QuotationServiceDetailDto> QuotationServices { get; set; }

        // Optional inspection information
        public InspectionDto Inspection { get; set; }
    }
    
    

    public class QuotationServiceDetailDto
    {
        public Guid QuotationServiceId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid ServiceId { get; set; }
        public bool IsSelected { get; set; }
        public bool IsAdvanced { get; set; }
        public bool IsRequired { get; set; } // Indicates if this is a required service

        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public DateTime CreatedAt { get; set; }

        public decimal DiscountValue { get; set; } = 0;


        public decimal FinalPrice => Price - DiscountValue;

        public Guid? AppliedPromotionId { get; set; }

        public virtual PromotionalCampaignDto? AppliedPromotion { get; set; }

        // Service details
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }

        // All parts for this service - customers cannot select individual parts
        public ICollection<QuotationPartCategoryDTO> PartCategories { get; set; }
    }
    public class QuotationPartCategoryDTO
    {
        public Guid PartCategoryId { get; set; }
        public string PartCategoryName { get; set; }

        public List<QuotationPart> Parts { get; set; } = new();
    }
    public class QuotationPart
    {
        public Guid QuotationServicePartId { get; set; }
        public Guid PartId { get; set; }
        public string PartName { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public bool IsSelected { get; set; }
    }
}
