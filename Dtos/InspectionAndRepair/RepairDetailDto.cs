using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.InspectionAndRepair
{
    public class RepairDetailDto
    {
        public Guid RepairOrderId { get; set; }
        public string VIN { get; set; }
        public string VehicleLicensePlate { get; set; }
        public VehicleDto Vehicle { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string Note { get; set; }

        public List<JobDetailDto> Jobs { get; set; }
       
    }
    public class RepairDto
    {
        public Guid RepairId { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? ActualTime { get; set; }
        public TimeSpan? EstimatedTime { get; set; }
    }
    public class JobDetailDto
    {
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public string ServiceName { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }

        public List<JobPartDto> Parts { get; set; }
        public RepairDto Repairs { get; set; }
        public List<TechnicianDto> Technicians { get; set; }
    }

    public class JobPartDto
    {
        public string PartName { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
