using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Statistical
{


    public class TechnicianStatisticDto
    {
        public float Quality { get; set; }
        public float Speed { get; set; }
        public float Efficiency { get; set; }
        public float Score { get; set; }

        public int NewJobs { get; set; }
        public int InProgressJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int OnHoldJobs { get; set; }

        public List<RecentJobDto> RecentJobs { get; set; } = new();
    }

    public class RecentJobDto
    {
        public string JobName { get; set; }
        public string LicensePlate { get; set; }
        public string Status { get; set; }
        public DateTime AssignedAt { get; set; }
    }
    
}
