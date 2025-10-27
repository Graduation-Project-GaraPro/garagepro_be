using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dtos.InspectionAndRepair;

namespace Dtos.RepairProgressDto
{
    public class JobDto
    {
        public Guid JobId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public decimal TotalAmount { get; set; }
        public string Note { get; set; } = string.Empty;
        public int Level { get; set; }

        // Repair information
        public RepairDto? Repair { get; set; }

        // Parts used in this job
        public List<PartDto> Parts { get; set; } = new List<PartDto>();

        // Technicians assigned to this job
        public List<TechnicianDto> Technicians { get; set; } = new List<TechnicianDto>();
    }
}
