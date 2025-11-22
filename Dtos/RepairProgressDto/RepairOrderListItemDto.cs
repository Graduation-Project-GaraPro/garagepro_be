
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.RepairProgressDto
{
    public class RepairOrderListItemDto
    {
        public Guid RepairOrderId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string RoType { get; set; } = string.Empty;
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal Cost { get; set; }
        public string PaidStatus { get; set; } = string.Empty;
        public string VehicleLicensePlate { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public bool IsArchived { get; set; } = false;
        public DateTime? ArchivedAt { get; set; }
        public List<LabelDto> Labels { get; set; } = new List<LabelDto>();
        public decimal ProgressPercentage { get; set; }
        public string ProgressStatus { get; set; } = string.Empty;
        public FeedbackDto FeedBacks { get; set; } = new FeedbackDto();
    }
}
