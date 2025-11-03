using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.InspectionAndRepair
{
    public class RepairResponseDto
    {
        public Guid RepairId { get; set; }
        public Guid RepairOrderId { get; set; }
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? ActualTime { get; set; }
        public TimeSpan? EstimatedTime { get; set; }
    }
}
