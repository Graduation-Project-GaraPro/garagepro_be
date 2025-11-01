using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public bool IsSelected { get; set; } 
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

        public bool IsSelected { get; set; } = true; // Parts are automatically selected when service is selected
        
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
        
        // Customer selects services they agree with
        public ICollection<CustomerQuotationServiceDto> SelectedServices { get; set; }
    }

    public class CustomerQuotationServiceDto
    {
        [Required]
        public Guid QuotationServiceId { get; set; }
        
        // For required services, customer cannot deselect
        // For optional services, customer can choose to select or deselect
    }
}