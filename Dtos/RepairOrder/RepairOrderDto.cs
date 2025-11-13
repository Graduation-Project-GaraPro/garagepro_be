using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BusinessObject.Enums;
using Dtos.RoBoard;
using Dtos.Vehicles;

namespace Dtos.RepairOrder
{
    public class RepairOrderDto
    {
        public Guid RepairOrderId { get; set; }
        
        [Required]
        public DateTime ReceiveDate { get; set; }
        
        [Required]
        public RoType RoType { get; set; }
        
        // New property to display the string name of the RoType
        public string RoTypeName => RoType.ToString();
        
        public DateTime? EstimatedCompletionDate { get; set; }
        
        public DateTime? CompletionDate { get; set; }
        
        public decimal Cost { get; set; }
        
        public decimal EstimatedAmount { get; set; }
        
        public decimal PaidAmount { get; set; }
        
        [Required]
        public PaidStatus PaidStatus { get; set; }
        
        public long? EstimatedRepairTime { get; set; }
        
        [MaxLength(500)]
        public string Note { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        // Archive Management
        public bool IsArchived { get; set; }
        
        public DateTime? ArchivedAt { get; set; }
        
        public string? ArchivedByUserId { get; set; }
        
        // Foreign keys
        public Guid BranchId { get; set; }
        
        public int StatusId { get; set; } // Changed from Guid to int
        
        public Guid VehicleId { get; set; }
        public VehicleDto Vehicle { get; set; }

        public string UserId { get; set; }
        
        public Guid? RepairRequestId { get; set; }
        
        // Enhanced properties for better display

        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public List<string> TechnicianNames { get; set; } = new List<string>();
        
        // Progress calculation
        public int TotalJobs { get; set; }
        public int CompletedJobs { get; set; }
        public decimal ProgressPercentage { get; set; }
    }
    
    
    public class CreateRoDto
    {
        [Required]
        public string CustomerId { get; set; }
        
        [Required]
        public Guid VehicleId { get; set; }
        
        [Required]
        public DateTime ReceiveDate { get; set; }
        
        [Required]
        public RoType RoType { get; set; }
        
        public DateTime? EstimatedCompletionDate { get; set; }
        
        // These fields will now be calculated based on selected services
        // public decimal EstimatedAmount { get; set; }
        // public long? EstimatedRepairTime { get; set; }
        
        [MaxLength(500)]
        public string Note { get; set; }
        
        // Optional RepairRequestId
        public Guid? RepairRequestId { get; set; }
        
        // Removed LabelId as labels should be accessed through OrderStatus
        // public int? LabelId { get; set; }
        
        // Add service selection
        public List<Guid> SelectedServiceIds { get; set; } = new List<Guid>();
    }

    public class UpdateRepairOrderDto
    {
        [Required]
        public DateTime ReceiveDate { get; set; }
        
        [Required]
        public RoType RoType { get; set; }
        
        public DateTime? EstimatedCompletionDate { get; set; }
        
        public DateTime? CompletionDate { get; set; }
        
        public decimal Cost { get; set; }
        
        public decimal EstimatedAmount { get; set; }
        
        public decimal PaidAmount { get; set; }
        
        [Required]
        public PaidStatus PaidStatus { get; set; }
        
        public long? EstimatedRepairTime { get; set; }
        
        [MaxLength(500)]
        public string Note { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        // Archive Management
        public bool IsArchived { get; set; }
        
        public DateTime? ArchivedAt { get; set; }
        
        public string? ArchivedByUserId { get; set; }
        
        // Foreign keys
        public Guid BranchId { get; set; }
        
        public int StatusId { get; set; } // Changed from Guid to int
        
        public Guid VehicleId { get; set; }
        
        public string UserId { get; set; }
    }
}