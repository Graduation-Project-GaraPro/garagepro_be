using System;

namespace Dtos.Job
{
    public class JobSummaryDto
    {
        public Guid JobId { get; set; }
        public Guid RepairOrderId { get; set; }
        public Guid? ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
