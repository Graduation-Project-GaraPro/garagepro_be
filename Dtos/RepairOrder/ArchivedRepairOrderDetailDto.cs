using System;
using System.Collections.Generic;
using BusinessObject.Enums;
using Dtos.RoBoard;
using Dtos.Vehicles;

namespace Dtos.RepairOrder
{
    public class ArchivedRepairOrderDetailDto
    {
        public Guid RepairOrderId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public RoType RoType { get; set; }
        public string RoTypeName => RoType.ToString();
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        
        // Financial Info
        public decimal Cost { get; set; }
        public decimal EstimatedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal MissingCost => EstimatedAmount - Cost;
        public decimal OutstandingAmount => EstimatedAmount - PaidAmount;
        public PaidStatus PaidStatus { get; set; }
        
        public long? EstimatedRepairTime { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Archive Info
        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public string ArchivedByUserId { get; set; }
        public string ArchivedByUserName { get; set; }
        public string ArchiveReason { get; set; }
        
        // Warranty Info
        public int? WarrantyMonths { get; set; }
        public DateTime? WarrantyStartAt { get; set; }
        public DateTime? WarrantyEndAt { get; set; }
        
        // Cancel Info
        public bool IsCancelled { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancelReason { get; set; }
        
        // Foreign keys
        public Guid BranchId { get; set; }
        public string BranchName { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public string StatusColor { get; set; }
        
        // Customer Info
        public string UserId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        
        // Vehicle Info
        public Guid VehicleId { get; set; }
        public VehicleDto Vehicle { get; set; }
        
        // Services
        public List<ArchivedRepairOrderServiceDto> Services { get; set; } = new List<ArchivedRepairOrderServiceDto>();
        
        // Inspections
        public List<ArchivedInspectionDto> Inspections { get; set; } = new List<ArchivedInspectionDto>();
        
        // Jobs
        public List<ArchivedJobDto> Jobs { get; set; } = new List<ArchivedJobDto>();
        
        // Payments
        public List<ArchivedPaymentDto> Payments { get; set; } = new List<ArchivedPaymentDto>();
        
        // Labels
        public List<RoBoardLabelDto> Labels { get; set; } = new List<RoBoardLabelDto>();
        
        // Progress
        public int TotalJobs { get; set; }
        public int CompletedJobs { get; set; }
        public decimal ProgressPercentage { get; set; }
        
        // Read-only flags
        public bool IsReadOnly => true;
        public bool CanEdit => false;
        public bool CanDelete => false;
        public bool CanChangeStatus => false;
        public bool CanAddPayment => false;
        public bool CanAddService => false;
        public bool CanAddInspection => false;
        public bool CanAddJob => false;
    }

    public class ArchivedRepairOrderServiceDto
    {
        public Guid RepairOrderServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public decimal ServicePrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => ServicePrice * Quantity;
        public List<ArchivedRepairOrderServicePartDto> Parts { get; set; } = new List<ArchivedRepairOrderServicePartDto>();
    }

    public class ArchivedRepairOrderServicePartDto
    {
        public Guid RepairOrderServicePartId { get; set; }
        public string PartName { get; set; }
        public string PartCode { get; set; }
        public decimal PartPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => PartPrice * Quantity;
    }

    public class ArchivedInspectionDto
    {
        public Guid InspectionId { get; set; }
        public string InspectionTypeName { get; set; }
        public string TechnicianName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class ArchivedJobDto
    {
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public string TechnicianName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        
        // Job Parts with warranty info
        public List<ArchivedJobPartDto> JobParts { get; set; } = new List<ArchivedJobPartDto>();
    }

    public class ArchivedJobPartDto
    {
        public Guid JobPartId { get; set; }
        public string PartName { get; set; }
        public string PartCode { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        
        // Warranty Info (directly from JobPart)
        public int? WarrantyMonths { get; set; }
        public DateTime? WarrantyStartAt { get; set; }
        public DateTime? WarrantyEndAt { get; set; }
    }

    public class ArchivedPaymentDto
    {
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Notes { get; set; }
    }
}
