using System;
using System.Collections.Generic;
using BusinessObject.Enums;
using BussinessObject;

namespace Dtos.PayOsDtos
{
    public class PaymentPreviewDto
    {
        public Guid RepairOrderId { get; set; }
        public decimal RepairOrderCost { get; set; }
        public decimal EstimatedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        // Customer and vehicle information
        public string CustomerName { get; set; }
        public string VehicleInfo { get; set; }
        public List<ServicePreviewDto> Services { get; set; } = new List<ServicePreviewDto>();
        public List<PartPreviewDto> Parts { get; set; } = new List<PartPreviewDto>();
        public List<QuotationInfoDto> Quotations { get; set; } = new List<QuotationInfoDto>();
    }

    public class ServicePreviewDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public decimal EstimatedDuration { get; set; }
    }

    public class PartPreviewDto
    {
        public Guid PartId { get; set; }
        public string PartName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class QuotationInfoDto
    {
        public Guid QuotationId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Status { get; set; }
    }

    public class PaymentSummaryDto
    {
        public Guid RepairOrderId { get; set; }
        public string CustomerName { get; set; }
        public string VehicleInfo { get; set; }
        public decimal RepairOrderCost { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal AmountToPay { get; set; }
        public PaidStatus PaidStatus { get; set; }
        public List<PaymentHistoryDto> PaymentHistory { get; set; } = new List<PaymentHistoryDto>();
    }

    public class PaymentHistoryDto
    {
        public long PaymentId { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string ProcessedBy { get; set; }
    }
}