using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;

namespace Dtos.RepairProgressDto
{
    public class RepairOrderProgressDto
    {
        public Guid RepairOrderId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string RoType { get; set; } = string.Empty;
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal Cost { get; set; }
        public decimal EstimatedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaidStatus { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;

        public bool IsArchived { get; set; } = false;

        public DateTime? ArchivedAt { get; set; }
        // Vehicle information
        public VehicleDto Vehicle { get; set; } = new VehicleDto();

        // Order status with labels
        public OrderStatusDto OrderStatus { get; set; } = new OrderStatusDto();

        // All jobs in this repair order
        public List<JobDto> Jobs { get; set; } = new List<JobDto>();

        public FeedbackDto FeedBacks { get; set; }

        // Progress calculation
        public decimal ProgressPercentage
        {
            get
            {
                if (Jobs.Count == 0) return 0;

                var completedJobs = Jobs.Count(j => j.Status == "Completed");
                var value = (decimal)completedJobs / Jobs.Count * 100;

                // Làm tròn về số chẵn
                return Math.Round(value, 0, MidpointRounding.ToEven);
                
            }
        }

        public string ProgressStatus
        {
            get
            {
                var percentage = ProgressPercentage;
                return percentage switch
                {
                    0 => "Not Start",
                    < 100 => "In Progress",
                    100 => "Completed",
                    _ => "Unknown"
                };
            }
        }
    }
}
