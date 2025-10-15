using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Branches
{
    public class ServiceDto
    {
        public Guid ServiceId { get; set; }
        public Guid ServiceCategoryId { get; set; }
        public Guid ServiceTypeId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceStatus { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal EstimatedDuration { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdvanced { get; set; }
    }

}
