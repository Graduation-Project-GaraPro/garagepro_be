using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Campaigns
{
    public class PromotionalCampaign
    {
        public Guid Id { get; set; }  // PK, UUID

        public string Name { get; set; } = string.Empty; // NOT NULL

        public string? Description { get; set; } // NULLABLE

        public CampaignType Type { get; set; } // ENUM('discount','seasonal','loyalty')

        public DiscountType DiscountType { get; set; } // ENUM('percentage','fixed','free_service')

        public decimal DiscountValue { get; set; } // >= 0

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true; // DEFAULT TRUE

        public decimal? MinimumOrderValue { get; set; } // NULLABLE

        public decimal? MaximumDiscount { get; set; } // NULLABLE

        public int? UsageLimit { get; set; } // NULLABLE

        public int UsedCount { get; set; } = 0; // DEFAULT 0

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Many-to-many
        public virtual ICollection<PromotionalCampaignService> PromotionalCampaignServices { get; set; }
            = new List<PromotionalCampaignService>();
        public virtual ICollection<VoucherUsage> VoucherUsages { get; set; }
        = new List<VoucherUsage>();
    }

    public enum CampaignType
    {
        Discount,   // 'discount'
        Seasonal,   // 'seasonal'
        Loyalty     // 'loyalty'
    }

    public enum DiscountType
    {
        Percentage,    // 'percentage'
        Fixed,         // 'fixed'
        FreeService    // 'free_service'
    }
}
