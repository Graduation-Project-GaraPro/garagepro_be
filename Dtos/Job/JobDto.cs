using System;
using System.ComponentModel.DataAnnotations;
using BusinessObject.Enums;

namespace Dtos.Job
{
    public class JobDto
    {
        public Guid JobId { get; set; }
        
        [Required]
        public Guid ServiceId { get; set; }
        
        [Required]
        public Guid RepairOrderId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string? JobName { get; set; }
        
        public JobStatus Status { get; set; }
        
        public DateTime? Deadline { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        [MaxLength(500)]
        public string? Note { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public int Level { get; set; }
        
        // Customer approval workflow properties
        public DateTime? SentToCustomerAt { get; set; }
        public DateTime? CustomerResponseAt { get; set; }
        public string? CustomerApprovalNote { get; set; }
        public string? AssignedByManagerId { get; set; }
        public DateTime? AssignedAt { get; set; }
        
        // Estimate expiration and revision properties
        public DateTime? EstimateExpiresAt { get; set; }
        public int RevisionCount { get; set; }
        public Guid? OriginalJobId { get; set; }
        public string? RevisionReason { get; set; }
    }
    
    public class CreateJobDto
    {
        [Required]
        public Guid ServiceId { get; set; }
        
        [Required]
        public Guid RepairOrderId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string? JobName { get; set; }
        
        public JobStatus Status { get; set; } = JobStatus.Pending;
        
        public DateTime? Deadline { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        [MaxLength(500)]
        public string? Note { get; set; }
        
        public int Level { get; set; } = 1;
        
        // Customer approval workflow properties
        public DateTime? SentToCustomerAt { get; set; }
        public DateTime? CustomerResponseAt { get; set; }
        public string? CustomerApprovalNote { get; set; }
        public string? AssignedByManagerId { get; set; }
        public DateTime? AssignedAt { get; set; }
    }
    
    public class UpdateJobDto
    {
        [Required]
        public Guid ServiceId { get; set; }
        
        [Required]
        public Guid RepairOrderId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string? JobName { get; set; }
        
        public JobStatus Status { get; set; }
        
        public DateTime? Deadline { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        [MaxLength(500)]
        public string? Note { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public int Level { get; set; }
        
        // Customer approval workflow properties
        public DateTime? SentToCustomerAt { get; set; }
        public DateTime? CustomerResponseAt { get; set; }
        public string? CustomerApprovalNote { get; set; }
        public string? AssignedByManagerId { get; set; }
        public DateTime? AssignedAt { get; set; }
        
        // Estimate expiration and revision properties
        public DateTime? EstimateExpiresAt { get; set; }
        public int RevisionCount { get; set; }
        public Guid? OriginalJobId { get; set; }
        public string? RevisionReason { get; set; }
    }
}