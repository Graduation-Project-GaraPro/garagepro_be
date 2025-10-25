using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Branches;

using BusinessObject.Campaigns;
using BusinessObject.Customers;


namespace BusinessObject
{
    public class Service
    {
        [Key]
        public Guid ServiceId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServiceCategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedDuration { get; set; } // in hours

        public bool IsActive { get; set; } = true;

        public bool IsAdvanced { get; set; } = false; // true được  chọn nhiều

        public Guid? BranchId { get; set; }


        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ServiceCategory ServiceCategory { get; set; }

        public virtual ICollection<RepairOrderService> RepairOrderServices { get; set; } = new List<RepairOrderService>();
        public virtual ICollection<ServiceInspection> ServiceInspections { get; set; } = new List<ServiceInspection>();
        public virtual ICollection<Job>? Jobs { get; set; } = new List<Job>();
        // Many-to-many
        public virtual ICollection<BranchService>? BranchServices { get; set; } = new List<BranchService>();
        public virtual ICollection<ServicePart>? ServiceParts { get; set; } = new List<ServicePart>();

        // Many-to-many
        public virtual ICollection<PromotionalCampaignService>? PromotionalCampaignServices { get; set; }
            = new List<PromotionalCampaignService>();

        // Quotation relationship
        public virtual ICollection<QuotationService>? QuotationServices { get; set; } = new List<QuotationService>();
        //request relationship
        public virtual ICollection<RequestService>? RequestServices { get; set; } = new List<RequestService>();

    }
}