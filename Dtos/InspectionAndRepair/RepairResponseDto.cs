using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        [JsonIgnore]
        public TimeSpan? ActualTime { get; set; }

        [JsonIgnore]
        public TimeSpan? EstimatedTime { get; set; }

        public string ActualTimeShort => FormatShort(ActualTime);
        public string EstimatedTimeShort => FormatShort(EstimatedTime);
        private static string FormatShort(TimeSpan? timeSpan)
        {
            if (!timeSpan.HasValue)
                return null;

            var ts = timeSpan.Value;
            int totalHours = (int)ts.TotalHours;
            int minutes = ts.Minutes;

            return $"{totalHours:D2}h {minutes:D2}m";
        }
    }
}