using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BusinessObject.Campaigns;
using System.ComponentModel.DataAnnotations.Schema;
using Dtos.Campaigns;

namespace Dtos.Quotations
{
    public class QuotationDto
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
        public ICollection<QuotationServiceDto> QuotationServices { get; set; }
        // Optional inspection information
        public InspectionDto Inspection { get; set; }
    }

    public class QuotationServiceDto
    {
        public Guid QuotationServiceId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid ServiceId { get; set; }
        public bool IsSelected { get; set; }
        public bool IsRequired { get; set; } // Indicates if this is a required service
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal DiscountValue { get; set; } = 0;

        
        public decimal FinalPrice => Price - DiscountValue;

        public Guid? AppliedPromotionId { get; set; }

        public virtual PromotionalCampaignDto? AppliedPromotion { get; set; }
        // Service details
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        // All parts for this service - customers cannot select individual parts
        public ICollection<QuotationServicePartDto> Parts { get; set; }
    }

    public class QuotationServicePartDto
    {
        public Guid QuotationServicePartId { get; set; }
        public Guid QuotationServiceId { get; set; }
        public Guid PartId { get; set; }
        public bool IsSelected { get; set; } // Customer's selection (what they approved/chose)
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        // Part details
        public string PartName { get; set; }
        public string PartDescription { get; set; }
    }

    public class CreateQuotationDto
    {
        public Guid? InspectionId { get; set; }
        public Guid? RepairOrderId { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "VehicleId is required")]
        public Guid VehicleId { get; set; }

        public string Note { get; set; }
        
        public ICollection<CreateQuotationServiceDto> QuotationServices { get; set; }
    }

    public class CreateQuotationServiceDto
    {
        [Required]
        public Guid ServiceId { get; set; }
        
        public bool IsRequired { get; set; } // Indicates if this is a required service

        public bool IsSelected { get; set; } = false;
        
        public ICollection<CreateQuotationServicePartDto> QuotationServiceParts { get; set; }
    }

    public class CreateQuotationServicePartDto
    {
        [Required]
        public Guid PartId { get; set; }

        // IsSelected = true means this part is pre-selected (recommended by technician or manager)
        // Customer can change selection via ProcessCustomerResponseAsync
        public bool IsSelected { get; set; } = false;
        
        public decimal Quantity { get; set; } = 1;
    }

    public class UpdateQuotationDto
    {
        public string Note { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class UpdateQuotationStatusDto
    {
        [Required]
        public string Status { get; set; } // Sent, Approved, Rejected
        
        public DateTime? CustomerResponseAt { get; set; }
    }

    public class CustomerQuotationResponseDto
    {
        [Required]
        public Guid QuotationId { get; set; }

        [Required]
        public string Status { get; set; } // Approved, Rejected

        public string? CustomerNote { get; set; }
        
        public ICollection<CustomerQuotationServiceDto> SelectedServices { get; set; }
    }

    public class CustomerQuotationServiceDto
    {
        [Required]
        public Guid QuotationServiceId { get; set; }
        
        // Simple list of selected part IDs - more efficient than sending full objects
        public List<Guid>? SelectedPartIds { get; set; }
        
        // Optional: Promotion applied to this service
        public Guid? AppliedPromotionId { get; set; }
    }

    public class UpdateQuotationDetailsDto
    {
        public string Note { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public decimal? DiscountAmount { get; set; }
        
        // Services to update/add/remove
        public ICollection<UpdateQuotationServiceDto> QuotationServices { get; set; }
    }

    public class UpdateQuotationServiceDto
    {
        public Guid? QuotationServiceId { get; set; } // Null if adding new service
        public Guid ServiceId { get; set; }
        public bool IsSelected { get; set; }
        // Note: IsRequired is NOT included - it's set during inspection and cannot be changed by manager
        public bool ShouldDelete { get; set; } = false; // Flag to delete this service
        
        // Parts to update/add/remove
        public ICollection<UpdateQuotationServicePartDto> QuotationServiceParts { get; set; }
    }

    public class UpdateQuotationServicePartDto
    {
        public Guid? QuotationServicePartId { get; set; } // Null if adding new part
        public Guid PartId { get; set; }
        // Manager can pre-select parts (recommend to customer)
        // Customer can change selection via ProcessCustomerResponseAsync
        public bool IsSelected { get; set; } = false;
        public decimal Quantity { get; set; }
        public bool ShouldDelete { get; set; } = false; // Flag to delete this part
    }
}