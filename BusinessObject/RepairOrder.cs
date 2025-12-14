using BusinessObject.Authentication;
using BusinessObject.Branches;
using BusinessObject.Customers;
using BusinessObject.Manager;
using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Campaigns;

namespace BusinessObject
{
    public class RepairOrder
    {
        [Key]
        public Guid RepairOrderId { get; set; } = Guid.NewGuid();

        [Required]
        public DateTime ReceiveDate { get; set; } = DateTime.UtcNow;

        [Required]
        public RoType RoType { get; set; } //repair request, quotation, emergency 

        public DateTime? EstimatedCompletionDate { get; set; }

        public DateTime? CompletionDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        public PaidStatus PaidStatus { get; set; }

        public long? EstimatedRepairTime { get; set; }

        [MaxLength(500)]
        public string Note { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsArchived { get; set; } = false;

        public DateTime? ArchivedAt { get; set; }


        public bool IsCancelled { get; set; } = false;
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }


        public string? ArchivedByUserId { get; set; }

        [Required]
        public Guid BranchId { get; set; }

        [Required]
        public int StatusId { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public string UserId { get; set; }

        public Guid? RepairRequestId { get; set; } = null;

        public Guid? FeedBackId { get; set; }

        // Navigation 
        [ForeignKey(nameof(RepairRequestId))]
        public virtual RepairRequest? RepairRequest { get; set; } 

        public virtual OrderStatus OrderStatus { get; set; }
        public virtual Branch Branch { get; set; }
        public virtual Vehicle Vehicle { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<RepairOrderService> RepairOrderServices { get; set; }
        public virtual ICollection<Inspection> Inspections { get; set; }
        public virtual ICollection<Job> Jobs { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual FeedBack? FeedBack { get; set; }
        public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();

        public CarPickupStatus CarPickupStatus { get; set; } = CarPickupStatus.None;

        // ?? One-to-many (VoucherUsage)
        public virtual ICollection<VoucherUsage>? VoucherUsages { get; set; }
            = new List<VoucherUsage>();
        
        // One-to-many relationship with Label (each RO has one label)
        public Guid? LabelId { get; set; }
        public virtual Label? Label { get; set; }
    }
}