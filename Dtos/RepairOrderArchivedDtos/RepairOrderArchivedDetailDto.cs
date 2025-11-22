using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Enums;

namespace Dtos.RepairOrderArchivedDtos
{
    public class RepairOrderArchivedDetailDto
    {
        public Guid RepairOrderId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public bool IsArchived { get; set; }

        // Vehicle + Branch
        public string LicensePlate { get; set; }
        public string BranchName { get; set; }
        public string BrandName { get; set; }
        public string ModelName { get; set; }

        // Một vài info tổng quan
        public decimal Cost { get; set; }
        public decimal EstimatedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public PaidStatus PaidStatus { get; set; }
        public string Note { get; set; }

        // Jobs
        public List<RepairOrderArchivedJobDto> Jobs { get; set; } = new();
    }

}
