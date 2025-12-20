using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dtos.InspectionAndRepair
{
    public class InspectionTechnicianDto
    {
        public Guid InspectionId { get; set; }
        public Guid RepairOrderId { get; set; }
        public Guid TechnicianId { get; set; }
        public InspectionStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string? CustomerConcern { get; set; }
        public string? Finding { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public RepairOrderDto? RepairOrder { get; set; }
        public List<ServiceInspectionDto> ServiceInspections { get; set; } = new();
        public Guid? ServiceCategoryId { get; set; }
        public string? CategoryName { get; set; }
        public bool CanUpdate { get; set; }
    }

    public class RepairOrderDto
    {
        public Guid RepairOrderId { get; set; }
        public VehicleDto? Vehicle { get; set; }
        public CustomerDto? Customer { get; set; }
        public List<RepairOrderServiceDto> Services { get; set; } = new();
        public List<RepairImageDto> RepairImages { get; set; } = new();
    }

    public class RepairImageDto
    {
        public Guid ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
    public class VehicleDto
    {
        public Guid VehicleId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;

        public VehicleBrandDto? Brand { get; set; }
        public VehicleModelDto? Model { get; set; }
    }

    public class VehicleBrandDto
    {
        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? Country { get; set; }
    }

    public class VehicleModelDto
    {
        public Guid ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public int ManufacturingYear { get; set; }
    }


    public class CustomerDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class RepairOrderServiceDto
    {
        public Guid RepairOrderServiceId { get; set; }
        public Guid ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public decimal ServicePrice { get; set; }
        public bool IsAdvanced { get; set; }
        public Guid? ServiceCategoryId { get; set; }
        public string? CategoryName { get; set; }
        public List<PartCategoryDto> AllPartCategories { get; set; } = new();
    }

    public class ServicePartDto
    {
        public Guid ServicePartId { get; set; }
        public Guid PartId { get; set; }
        public string? PartName { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class ServiceInspectionDto
    {
        public Guid ServiceInspectionId { get; set; }
        public Guid ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public ConditionStatus ConditionStatus { get; set; }
        public Guid? ServiceCategoryId { get; set; }
        public string? CategoryName { get; set; }
        public bool IsAdvanced { get; set; }
        public List<PartCategoryDto> AllPartCategories { get; set; } = new();
        public List<PartInspectionDto> SuggestedParts { get; set; } = new();
    }

    public class PartInspectionDto
    {
        public Guid PartInspectionId { get; set; }
        public Guid PartId { get; set; }
        public string? PartName { get; set; }
        public Guid PartCategoryId { get; set; }
        public string? CategoryName { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; } = 1;
    }
    public class AddServiceToInspectionRequest
    {
        [Required]
        public Guid ServiceId { get; set; }
    }
    public class RemovePartCategoryFromServiceRequest
    {
        [Required]
        public Guid ServiceInspectionId { get; set; }

        [Required]
        public Guid PartCategoryId { get; set; }
    }
    public class AllServiceDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAdvanced { get; set; }
        public Guid? ServiceCategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
    public class ServiceCategoryDto
    {
        public Guid ServiceCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
    public class PartCategoryDto
    {
        public Guid PartCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid ModelId { get; set; }
        public List<ServicePartDto> Parts { get; set; } = new();
    }

}
