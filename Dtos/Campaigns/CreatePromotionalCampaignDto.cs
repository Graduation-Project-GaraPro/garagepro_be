using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Campaigns;

namespace Dtos.Campaigns
{
    public class CreatePromotionalCampaignDto
    {
        [Required(ErrorMessage = "Campaign name is required")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Campaign type is required")]
        public CampaignType Type { get; set; }

        [Required(ErrorMessage = "Discount type is required")]
        public DiscountType DiscountType { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0")]
        public decimal DiscountValue { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(0, double.MaxValue, ErrorMessage = "Minimum order value must be >= 0")]
        public decimal? MinimumOrderValue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Maximum discount must be >= 0")]
        public decimal? MaximumDiscount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Usage limit must be >= 1")]
        public int? UsageLimit { get; set; }

        [MinLength(1, ErrorMessage = "At least one service must be selected")]
        public List<Guid> ServiceIds { get; set; } = new();
    }
}
