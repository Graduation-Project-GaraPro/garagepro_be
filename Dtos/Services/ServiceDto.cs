using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Branches;

namespace Dtos.Services
{
    public class ServiceDto
    {
        public Guid ServiceId { get; set; }
        public Guid ServiceCategoryId { get; set; }
        public string ServiceName { get; set; }      
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal EstimatedDuration { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdvanced { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual GetCategoryForServiceDto ServiceCategory { get; set; }

        public virtual ICollection<BranchServiceRelatedDto>? Branches { get; set; } = new List<BranchServiceRelatedDto>();
        public virtual ICollection<PartServiceRelatedDto>? Parts { get; set; } = new List<PartServiceRelatedDto>();

    }
}
