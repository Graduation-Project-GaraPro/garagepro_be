using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Campaigns
{
    public class PromotionalCampaignService
    {
        [Key]
        public Guid PromotionalCampaignServiceId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PromotionalCampaignId { get; set; }

        [Required]
        public Guid ServiceId { get; set; }

        // Navigation properties
        public virtual PromotionalCampaign PromotionalCampaign { get; set; }
        public virtual Service Service { get; set; }
    }
}