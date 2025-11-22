using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Campaigns;
using BusinessObject;

namespace Dtos.Campaigns
{
    public class ServicePromotionResponse
    {
        public Service Service { get; set; }
        public PromotionalCampaign? BestPromotion { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal FinalPrice { get; set; }
    }
}
