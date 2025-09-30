using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;
using BusinessObject.Manager;

namespace BusinessObject
{
    public class RepairOrder
    {
        [Key]
        public Guid RepairOrderId { get; set; } = Guid.NewGuid();

        [Required]
        public DateTime ReceiveDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string RepairOrderType { get; set; }

        public DateTime? EstimatedCompletionDate { get; set; }

        public DateTime? CompletionDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaidStatus { get; set; }

        public long? EstimatedRepairTime { get; set; }

        [MaxLength(500)]
        public string Note { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Archive Management
        public bool IsArchived { get; set; } = false;

        public DateTime? ArchivedAt { get; set; }

        [MaxLength(500)]
        public string ArchiveReason { get; set; }

        public string ArchivedByUserId { get; set; }

        [Required]
        public Guid BranchId { get; set; }

        [Required]
        public Guid StatusId { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public Guid RepairRequestId { get; set; }

        public Guid? FeedBackId { get; set; }

        // Navigation property
        public virtual OrderStatus OrderStatus { get; set; }
        public virtual Branch Branch { get; set; }
        public virtual Vehicle Vehicle { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<RepairOrderService> RepairOrderServices { get; set; }
        public virtual ICollection<Inspection> Inspections { get; set; }
        public virtual ICollection<Job> Jobs { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual FeedBack? FeedBack { get; set; }
    }
}