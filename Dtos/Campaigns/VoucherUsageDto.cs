using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;
using BusinessObject.Campaigns;
using Dtos.InspectionAndRepair;

namespace Dtos.Campaigns
{
    public class VoucherUsageDto
    {
        
        public Guid Id { get; set; } = Guid.NewGuid();

        
        public string CustomerId { get; set; }

        
        public Guid CampaignId { get; set; }

      
        public Guid RepairOrderId { get; set; }

        public DateTime? UsedAt { get; set; }

        // 🔗 Navigation properties
        public virtual CustomerDto Customer { get; set; } = null!;
      
        public virtual RepairOrderDto RepairOrder { get; set; } = null!;
    }
}
