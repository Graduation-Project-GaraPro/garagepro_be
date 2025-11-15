using BusinessObject.Authentication;
using BussinessObject;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Payment
    {
        [Key]
        public long PaymentId { get; set; }
        public Guid RepairOrderId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string? ProviderDesc { get; set; }
        public string? ProviderCode { get; set; }
        public string? CheckoutUrl { get; set; }
        public PaymentMethod Method { get; set; } // Enum: PayOs, Cash
        public PaymentStatus Status { get; set; } // Paid, Unpaid, Refunded
        public DateTime PaymentDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual RepairOrder RepairOrder { get; set; } = null!;
    }
}