using System;
using System.Collections.Generic;
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
        
        public JobStatus Status { get; set; } = JobStatus.Pending;
        
        public DateTime? Deadline { get; set; }
        
        [MaxLength(500)]
        public string? Note { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        // Manager-Technician communication properties
        public string? AssignedByManagerId { get; set; }
        public DateTime? AssignedAt { get; set; }
        
        // Technician information
        public string? TechnicianName { get; set; }
        
        // Parts associated with this job
        public ICollection<JobPartDto> Parts { get; set; } = new List<JobPartDto>();
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
        
        // Manager-Technician communication properties
        public string? AssignedByManagerId { get; set; }
        public DateTime? AssignedAt { get; set; }
    }
    
    public class UpdateJobDto
    {
        public JobStatus? Status { get; set; }
        
        [MaxLength(500)]
        public string? Note { get; set; }
        
        public DateTime? Deadline { get; set; }
    }
    
    // Technician Schedule DTOs
    public class TechnicianScheduleDto
    {
        public Guid JobId { get; set; }
        
        public string JobName { get; set; } = string.Empty;
        
        public Guid TechnicianId { get; set; }
        
        public string TechnicianName { get; set; } = string.Empty;
        
        public JobStatus Status { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? Deadline { get; set; }
        
        public double EstimatedDuration { get; set; } // in minutes
        
        public double ActualDuration { get; set; } // in minutes
        
        public bool IsOverdue { get; set; }
        
        public Guid RepairOrderId { get; set; }
        
        public string VehicleLicensePlate { get; set; } = string.Empty;
    }
    
    public class TechnicianScheduleFilterDto
    {
        public Guid? TechnicianId { get; set; }
        
        public JobStatus? Status { get; set; }
        
        public DateTime? FromDate { get; set; }
        
        public DateTime? ToDate { get; set; }
        
        public int? PriorityLevel { get; set; }
        
        public bool? IsOverdueOnly { get; set; }
    }
    
    public class TechnicianWorkloadDto
    {
        public Guid TechnicianId { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        
        public string FullName { get; set; } = string.Empty;
        
        public int TotalJobs { get; set; }
        
        public int CompletedJobs { get; set; }
        
        public int InProgressJobs { get; set; }
        
        public int PendingJobs { get; set; }
        
        public int OverdueJobs { get; set; }
        
        public float AverageCompletionTime { get; set; } // in minutes
        
        public float Efficiency { get; set; } // 0-100
        
        public float Quality { get; set; } // 0-100
        
        public float Speed { get; set; } // 0-100
        
        public float Score { get; set; } // 0-100 (overall performance score)
    }
}