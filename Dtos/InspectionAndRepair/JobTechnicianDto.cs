using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.InspectionAndRepair
{
   public class JobTechnicianDto
    {
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public string Status { get; set; }
        public DateTime? Deadline { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Level { get; set; }
        public string ServiceName { get; set; }

        public Guid? RepairOrderId { get; set; }

        public VehicleDto Vehicle { get; set; }
        public CustomerDto Customer { get; set; }
        public List<PartDto> Parts { get; set; }
        public RepairDto Repair { get; set; }
    }
   

    public class PartDto
    {
        public Guid PartId { get; set; }
        public string PartName { get; set; }
    }

    public class RepairsDto
    {
        public Guid RepairId { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? ActualTime { get; set; }
        public TimeSpan? EstimatedTime { get; set; }
        public string Notes { get; set; }
    }
    public class JobStatusUpdateDto
    {
        public Guid JobId { get; set; }
        public JobStatus JobStatus { get; set; }
    }
}
