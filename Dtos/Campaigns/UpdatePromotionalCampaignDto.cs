using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Campaigns
{
    public class UpdatePromotionalCampaignDto : CreatePromotionalCampaignDto
    {
        public Guid Id { get; set; }
    }
}
