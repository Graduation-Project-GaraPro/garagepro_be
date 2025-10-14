using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Campaigns
{
    public class ServiceRelatedToCampaignDto
    {
        
        public Guid ServiceId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServiceCategoryId { get; set; }



        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; }


        [MaxLength(500)]
        public string Description { get; set; }

        
        public decimal Price { get; set; }
       
        public decimal EstimatedDuration { get; set; } // in hours

        public bool IsActive { get; set; } = true;

        public bool IsAdvanced { get; set; } = false;

    }
}
