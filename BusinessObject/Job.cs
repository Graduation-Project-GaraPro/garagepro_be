
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;
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

        public string? AssignedByManagerId { get; set; } 
        public DateTime? AssignedAt { get; set; }

        public int RevisionCount { get; set; } = 0; 
        public Guid? OriginalJobId { get; set; } 
        public string? RevisionReason { get; set; }

        // navigation
        public virtual Service Service { get; set; }
        public virtual RepairOrder RepairOrder { get; set; }
        public virtual ICollection<JobPart> JobParts { get; set; }
        public virtual ICollection<JobTechnician> JobTechnicians { get; set; } = new List<JobTechnician>(); 
        public virtual Repair Repair { get; set; }
        public virtual Job OriginalJob { get; set; } // Navigation property for the original job
    }
}