using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Revenue
{
    public class RevenueFiltersDto
    {
        public string Period { get; set; } // "daily" | "monthly" | "yearly"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? TechnicianId { get; set; }
        public string? ServiceType { get; set; }
    }

}
