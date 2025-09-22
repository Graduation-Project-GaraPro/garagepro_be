using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [Required]
        public Guid BranchId { get; set; }

        [Required]
        public Guid StatusId { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid RepairRequestId { get; set; }

        // Navigation property
        public virtual OrderStatus OrderStatus { get; set; }
        public virtual Branch Branch { get; set; }
        public virtual Vehicle Vehicle { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual ICollection<RepairOrderService> RepairOrderServices { get; set; }
        public virtual ICollection<Inspection> Inspections { get; set; }
        public virtual ICollection<Job> Jobs { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}