using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Enums;

namespace Dtos.Bills
{
    // Dtos/RepairOrders/RepairOrderPaymentDto.cs
    public class RepairOrderPaymentDto
    {
        public Guid RepairOrderId { get; set; }
        public DateTime ReceiveDate { get; set; }

        public decimal Cost { get; set; }
        public decimal EstimatedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public PaidStatus PaidStatus { get; set; }

        public VehicleDto Vehicle { get; set; }
        public List<ApprovedQuotationDto> ApprovedQuotations { get; set; } = new();
    }

    // Dtos/RepairOrders/VehicleDto.cs
    public class VehicleDto
    {
        public Guid VehicleId { get; set; }
        public string LicensePlate { get; set; }
        public string VIN { get; set; }
        public int Year { get; set; }
        public long? Odometer { get; set; }

        public string BrandName { get; set; }
        public string ModelName { get; set; }
    }

    // Dtos/RepairOrders/ApprovedQuotationDto.cs
    public class ApprovedQuotationDto
    {
        public Guid QuotationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetAmount => TotalAmount - DiscountAmount;
        public string? Note { get; set; }
        public string? CustomerNote { get; set; }

        public List<QuotationServiceDto> Services { get; set; } = new();
    }

    // Dtos/RepairOrders/QuotationServiceDto.cs
    public class QuotationServiceDto
    {
        public Guid QuotationServiceId { get; set; }
        public Guid ServiceId { get; set; }
        public string? ServiceName { get; set; }

        public bool IsSelected { get; set; }
        public bool IsRequired { get; set; }

        public decimal Price { get; set; }

        public List<QuotationServicePartDto> Parts { get; set; } = new();
    }

    // Dtos/RepairOrders/QuotationServicePartDto.cs
    public class QuotationServicePartDto
    {
        public Guid QuotationServicePartId { get; set; }
        public Guid PartId { get; set; }
        public string PartName { get; set; }
        public decimal Price { get; set; }
        public bool IsSelected { get; set; }
        public decimal Quantity { get; set; }
    }

}
