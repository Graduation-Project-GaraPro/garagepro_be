using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Campaigns
{
    public class CustomerPromotionResponse
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
        public List<CustomerPromotionDto> Promotions { get; set; } = new List<CustomerPromotionDto>();
        public CustomerPromotionDto? BestPromotion { get; set; }
    }
}
