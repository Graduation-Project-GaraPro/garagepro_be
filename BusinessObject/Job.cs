﻿using BusinessObject.Enums;
using BusinessObject.Technician;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Job
    {
        [Key]
        public Guid JobId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServiceId { get; set; }

        [Required]
        public Guid RepairOrderId { get; set; }

        [Required]
        [MaxLength(100)]
        public string JobName { get; set; }

        public JobStatus Status { get; set; } = JobStatus.Pending;

        public DateTime? Deadline { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(500)]
        public string Note { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int Level { get; set; }

        // Customer approval workflow properties
        public DateTime? SentToCustomerAt { get; set; }
        public DateTime? CustomerResponseAt { get; set; }
        public string? CustomerApprovalNote { get; set; }
        public string? AssignedByManagerId { get; set; }  // UserId of manager who assigned
        public DateTime? AssignedAt { get; set; }
        
        // Estimate expiration and revision properties
        public DateTime? EstimateExpiresAt { get; set; }
        public int RevisionCount { get; set; } = 0;  // Track how many times estimate was revised
        public Guid? OriginalJobId { get; set; }  // Link to original job if this is a revision
        public string? RevisionReason { get; set; }  // Why the estimate was revised

        // Quotation reference
        public Guid? QuotationId { get; set; } // Link to the quotation this job was created from

        // Navigation properties
        public virtual Service Service { get; set; }
        public virtual RepairOrder RepairOrder { get; set; }
        public virtual ICollection<JobPart> JobParts { get; set; }
        public virtual ICollection<JobTechnician> JobTechnicians { get; set; } = new List<JobTechnician>(); // Thêm quan hệ với JobTechnician
        public virtual ICollection<Repair> Repairs { get; set; } = new List<Repair>();
        public virtual Quotation Quotation { get; set; } // Add this line
    }
}