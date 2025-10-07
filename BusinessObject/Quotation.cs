using BusinessObject.Authentication;
using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Quotation
    {
        [Key]
        public Guid QuotationId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid InspectionId { get; set; } // Link to inspection instead of job

        [Required]
        public string UserId { get; set; } // Manager who created the quotation

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SentAt { get; set; } // When sent to customer

        public DateTime? ResponseAt { get; set; } // When customer responded

        public QuotationStatus Status { get; set; } = QuotationStatus.Draft;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(1000)]
        public string CustomerNote { get; set; } // Note for customer

        [Required]
        [MaxLength(1000)]
        public string ChangeRequestDetails { get; set; } // Details if customer requests changes

        public DateTime? EstimateExpiresAt { get; set; } // When the quotation expires

        // Revision tracking
        public int RevisionNumber { get; set; } = 0;
        public Guid? OriginalQuotationId { get; set; } // For tracking revisions

        // Navigation properties
        public virtual Inspection Inspection { get; set; } // Link to inspection
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<QuotationService> QuotationServices { get; set; } = new List<QuotationService>();
        public virtual Job Job { get; set; } // Reverse navigation to job
    }
}