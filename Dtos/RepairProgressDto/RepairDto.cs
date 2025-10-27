using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.RepairProgressDto
{
    public class RepairDto
    {
        public Guid RepairId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? ActualTime { get; set; }
        public TimeSpan? EstimatedTime { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
