using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Enums;

namespace Dtos.RepairOrderArchivedDtos
{
    public class RepairOrderArchivedJobDto
    {
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public JobStatus Status { get; set; }
        public DateTime? Deadline { get; set; }
        public decimal TotalAmount { get; set; }


        public decimal ServicePrice {  get; set; }

        public decimal DiscountValue {  get; set; } = 0;

        // Repair info – chỉ lấy StartTime, EndTime, Notes
        public RepairOrderArchivedRepairDto Repair { get; set; }

        // Technicians làm job này
        public List<RepairOrderArchivedTechnicianDto> Technicians { get; set; } = new();

        // Parts sử dụng cho job này
        public List<RepairOrderArchivedJobPartDto> Parts { get; set; } = new();
    }

}
