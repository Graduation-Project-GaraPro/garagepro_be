using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Quotations
{
    public class QuotationDto
    {
        public Guid QuotationId { get; set; }
        public Guid InspectionId { get; set; }
        public string UserId { get; set; }
        public Guid VehicleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentToCustomerAt { get; set; }
        public DateTime? CustomerResponseAt { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Note { get; set; }
        public DateTime? ExpiresAt { get; set; }
        
        // Navigation properties
        public string CustomerName { get; set; }
        public string VehicleInfo { get; set; }
        public ICollection<QuotationServiceDto> QuotationServices { get; set; }
        // Remove direct QuotationParts and add QuotationServiceParts
        // public ICollection<QuotationPartDto> QuotationParts { get; set; }
        public ICollection<QuotationServicePartDto> QuotationServiceParts { get; set; }
    }

    public class QuotationServiceDto
    {
        public Guid QuotationServiceId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid ServiceId { get; set; }
        public bool IsSelected { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Service details
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        
        // Add QuotationServiceParts for this service
        public ICollection<QuotationServicePartDto> QuotationServiceParts { get; set; }
    }

    public class QuotationServicePartDto
    {
        public Guid QuotationServicePartId { get; set; }
        public Guid QuotationServiceId { get; set; }
        public Guid PartId { get; set; }
        public bool IsSelected { get; set; } // Customer selection
        // Add the property for manager recommendation
        public bool IsRecommended { get; set; } // Manager recommendation
        public string RecommendationNote { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Part details
        public string PartName { get; set; }
        public string PartDescription { get; set; }
    }

    public class CreateQuotationDto
    {
        [Required]
        public Guid InspectionId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        public string Note { get; set; }
        
        public ICollection<CreateQuotationServiceDto> QuotationServices { get; set; }
        // Remove direct QuotationParts and add QuotationServiceParts
        // public ICollection<CreateQuotationPartDto> QuotationParts { get; set; }
    }

    public class CreateQuotationServiceDto
    {
        [Required]
        public Guid ServiceId { get; set; }

        public bool IsSelected { get; set; } = false;

        [Required]
        public decimal Price { get; set; }

        public decimal Quantity { get; set; } = 1;
        
        // Add QuotationServiceParts for this service
        public ICollection<CreateQuotationServicePartDto> QuotationServiceParts { get; set; }
    }

    public class CreateQuotationServicePartDto
    {
        [Required]
        public Guid PartId { get; set; }

        public bool IsSelected { get; set; } = false; // Customer selection
        
        // Add the property for manager recommendation
        public bool IsRecommended { get; set; } = false; // Manager recommendation
        public string RecommendationNote { get; set; }

        [Required]
        public decimal Price { get; set; }

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

        public string CustomerNote { get; set; }
        
        public ICollection<CustomerQuotationServiceDto> SelectedServices { get; set; }
        // Change to use QuotationServicePart instead of direct QuotationPart
        // public ICollection<CustomerQuotationPartDto> SelectedParts { get; set; }
        public ICollection<CustomerQuotationServicePartDto> SelectedServiceParts { get; set; }
    }

    public class CustomerQuotationServiceDto
    {
        [Required]
        public Guid QuotationServiceId { get; set; }
    }

    public class CustomerQuotationServicePartDto
    {
        [Required]
        public Guid QuotationServicePartId { get; set; }
    }
}