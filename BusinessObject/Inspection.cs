using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Enums;
namespace BusinessObject
{
    public class Inspection
    {
        [Key]
        public Guid InspectionId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RepairOrderId { get; set; }

        public Guid? TechnicianId { get; set; }

        public InspectionStatus Status { get; set; } = InspectionStatus.New;

        [MaxLength(500)]
        public string CustomerConcern { get; set; }

        [MaxLength(500)]
        public string? Finding { get; set; }

        public IssueRating IssueRating { get; set; } = IssueRating.Fair;

        [MaxLength(500)]
        public string? Note { get; set; }

        public decimal InspectionPrice { get; set; } = 0;
        
        public InspectionType InspectionType { get; set; } = InspectionType.Full;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual RepairOrder RepairOrder { get; set; }
        public virtual InspectionAndRepair.Technician Technician { get; set; } // Thêm quan hệ với Technician

        public virtual ICollection<ServiceInspection> ServiceInspections { get; set; }
        public virtual ICollection<PartInspection> PartInspections { get; set; }
        public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
    }
}