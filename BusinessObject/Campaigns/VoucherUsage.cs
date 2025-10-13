using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;

namespace BusinessObject.Campaigns
{
    public class VoucherUsage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string CustomerId { get; set; }

        [Required]
        public Guid CampaignId { get; set; }

        [Required]
        public Guid RepairOrderId { get; set; }

        public DateTime? UsedAt { get; set; }

        // 🔗 Navigation properties
        public virtual ApplicationUser Customer { get; set; } = null!;
        public virtual PromotionalCampaign Campaign { get; set; } = null!;
        public virtual RepairOrder RepairOrder { get; set; } = null!;
    }

}
