using BusinessObject.Enums;
using System;
using System.Collections.Generic;

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
        public Guid BrandId { get; set; }
        public Guid ModelId { get; set; }
        public Guid ColorId { get; set; }
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
}
