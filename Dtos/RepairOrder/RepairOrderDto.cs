using System;
using System.ComponentModel.DataAnnotations;

namespace Dtos.RepairOrder
{
    public class RepairOrderDto
    {
        public Guid RepairOrderId { get; set; }
        
        [Required]
        public DateTime ReceiveDate { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string RepairOrderType { get; set; }
        
        public DateTime? EstimatedCompletionDate { get; set; }
        
        public DateTime? CompletionDate { get; set; }
        
        public decimal Cost { get; set; }
        
        public decimal EstimatedAmount { get; set; }
        
        public decimal PaidAmount { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string PaidStatus { get; set; }
        
        public long? EstimatedRepairTime { get; set; }
        
        [MaxLength(500)]
        public string Note { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        // Archive Management
        public bool IsArchived { get; set; }
        
        public DateTime? ArchivedAt { get; set; }
        
        public string ArchivedByUserId { get; set; }
        
        // Foreign keys
        public Guid BranchId { get; set; }
        
        public Guid StatusId { get; set; }
        
        public Guid VehicleId { get; set; }
        
        public string UserId { get; set; }
        
        public Guid RepairRequestId { get; set; }
    }
    
    public class CreateRepairOrderDto
    {
        [Required]
        public DateTime ReceiveDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [MaxLength(50)]
        public string RepairOrderType { get; set; }
        
        public DateTime? EstimatedCompletionDate { get; set; }
        
        public decimal Cost { get; set; }
        
        public decimal EstimatedAmount { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string PaidStatus { get; set; } = "Unpaid";
        
        public long? EstimatedRepairTime { get; set; }
        
        [MaxLength(500)]
        public string Note { get; set; }
        
        // Foreign keys
        public Guid BranchId { get; set; }
        
        public Guid StatusId { get; set; }
        
        public Guid VehicleId { get; set; }
        
        public string UserId { get; set; }
        
        public Guid RepairRequestId { get; set; }
    }
    
    public class UpdateRepairOrderDto
    {
        [Required]
        public DateTime ReceiveDate { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string RepairOrderType { get; set; }
        
        public DateTime? EstimatedCompletionDate { get; set; }
        
        public DateTime? CompletionDate { get; set; }
        
        public decimal Cost { get; set; }
        
        public decimal EstimatedAmount { get; set; }
        
        public decimal PaidAmount { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string PaidStatus { get; set; }
        
        public long? EstimatedRepairTime { get; set; }
        
        [MaxLength(500)]
        public string Note { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        // Archive Management
        public bool IsArchived { get; set; }
        
        public DateTime? ArchivedAt { get; set; }
        
        public string ArchivedByUserId { get; set; }
        
        // Foreign keys
        public Guid BranchId { get; set; }
        
        public Guid StatusId { get; set; }
        
        public Guid VehicleId { get; set; }
        
        public string UserId { get; set; }
    }
}