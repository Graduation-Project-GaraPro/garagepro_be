using Dtos.InspectionAndRepair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.RepairHistory
{
    
    public class RepairHistoryDto
    {
        public VehicleDto Vehicle { get; set; } 
        public CustomerDto Owner { get; set; }
        public int RepairCount { get; set; }
        public List<JobHistoryDto> CompletedJobs { get; set; } = new();
    }

    public class JobHistoryDto
    {
        public string JobName { get; set; }
        public string Note { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? Deadline { get; set; }
        public int Level { get; set; }
        public string CustomerIssue { get; set; } // Notes từ RepairOrder
        public List<JobPartDto> JobParts { get; set; } = new();
        public List<ServiceDto> Services { get; set; } = new();
    }

    public class JobPartDto
    {
        public string PartName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class ServiceDto
    {
        public string ServiceName { get; set; }
        public decimal ServicePrice { get; set; }
        public decimal ActualDuration { get; set; }
        public string Notes { get; set; }
    }

}
