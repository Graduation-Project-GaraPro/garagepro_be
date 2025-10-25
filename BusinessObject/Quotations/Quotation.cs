using BusinessObject.Authentication;
using BusinessObject.Branches;
using BusinessObject.Customers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Quotations
{
    public class Quotation
    {
        [Key]
        public Guid QuotationID { get; set; } = new Guid();

        [Required]
        public Guid RepairRequestID { get; set; }



        [Required]
        public Guid BranchID { get; set; }

        // Chi phí
        [Column(TypeName = "decimal(18,2)")]
        public decimal ServiceCost { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PartsCost { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalAmount { get; set; }
        public Status Status { get; set; } = Status.Pending; // Pending, Approved,update, Rejected, Expired

        public DateTime? ValidUntil { get; set; }

        public string? Notes { get; set; }
        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public String? ApprovedBy { get; set; }

        // Navigation Properties
        [ForeignKey("RepairRequestID")]
        public virtual RepairRequest RepairRequest { get; set; }

        [ForeignKey("BranchID")]
        public virtual Branch Branch { get; set; }

        [ForeignKey("ApprovedBy")]
        public virtual ApplicationUser ApprovedByUser { get; set; }

        public virtual ICollection<QuotationItem> QuotationItems { get; set; } = new List<QuotationItem>();
    }
    public enum Status
    {
        Pending,
        Approved,
        Rejected,
        Updated,
        Expired
    }
}

