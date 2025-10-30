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
        public string StatusText { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? CustomerConcern { get; set; }
        public string? Finding { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public RepairOrderDto? RepairOrder { get; set; }
        public List<ServiceInspectionDto> ServiceInspections { get; set; } = new();
       // public List<PartInspectionDto> PartInspections { get; set; } = new();

        public bool CanUpdate { get; set; }
    }

    public class RepairOrderDto
    {
        public Guid RepairOrderId { get; set; }
        public VehicleDto? Vehicle { get; set; }
        public CustomerDto? Customer { get; set; }
        public List<RepairOrderServiceDto> Services { get; set; } = new();
    }

    public class VehicleDto
    {
        public Guid VehicleId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;

        // Navigation objects instead of just IDs
        public VehicleBrandDto? Brand { get; set; }
        public VehicleModelDto? Model { get; set; }
        public VehicleColorDto? Color { get; set; }
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

    public class VehicleColorDto
    {
        public Guid ColorId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string? HexCode { get; set; }
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
        public decimal  ActualDuration { get; set; }
        public string? Notes { get; set; }
        public bool IsAdvanced { get; set; }
        public List<RepairOrderServicePartDto> Parts { get; set; } = new();
        public List<ServicePartDto> AllServiceParts { get; set; } = new();
    }
    public class ServicePartDto
    {
        public Guid ServicePartId { get; set; }
        public Guid PartId { get; set; }
        public string? PartName { get; set; }
        //public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class RepairOrderServicePartDto
    {
        public Guid RepairOrderServicePartId { get; set; }
        public Guid PartId { get; set; }
        public string? PartName { get; set; }
        public string? Notes { get; set; }
    }
    public class ServiceInspectionDto
    {
        public Guid ServiceInspectionId { get; set; }
        public Guid ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public ConditionStatus ConditionStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        // <-- Suggested parts for this service
        public List<PartInspectionDto> SuggestedParts { get; set; } = new();
    }


    public class PartInspectionDto
    {
        public Guid PartInspectionId { get; set; }
        public Guid PartId { get; set; }
        public string? PartName { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class AddServiceToInspectionRequest
    {
        [Required]
        public Guid ServiceId { get; set; }
    }
    public class AllServiceDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAdvanced { get; set; }
    }

}
