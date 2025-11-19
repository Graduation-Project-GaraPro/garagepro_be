using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Campaigns;

namespace Dtos.Campaigns
{
    public class CustomerPromotionDto
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

        // Additional fields for UI
        public decimal CalculatedDiscount { get; set; } // Giá trị discount thực tế
        public bool IsEligible { get; set; } // Có đủ điều kiện sử dụng không
        public string DiscountDisplayText { get; set; } = string.Empty; // Text hiển thị discount
        public string EligibilityMessage { get; set; } = string.Empty; // Thông báo điều kiện
        public decimal FinalPriceAfterDiscount { get; set; } // Giá cuối cùng sau discount
    }
}
