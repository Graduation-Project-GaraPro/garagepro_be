using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Campaigns;

namespace Dtos.Campaigns
{
    public class PromotionalCampaignDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CampaignType Type { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public decimal? MinimumOrderValue { get; set; }
        public decimal? MaximumDiscount { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<ServiceRelatedToCampaignDto> Services { get; set; } = new();

        public virtual ICollection<VoucherUsageDto> VoucherUsages { get; set; }
       = new List<VoucherUsageDto>();
    }
}
