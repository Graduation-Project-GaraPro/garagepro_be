using BusinessObject.Enums;
using System;
using System.Collections.Generic;

namespace Dtos.RepairOrder
{
    public class RepairOrderListItemDto
    {
        public Guid RepairOrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        // Branch / Status / User / Vehicle basic info
        public Guid BranchId { get; set; }
        public string BranchName { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public string LabelName { get; set; }

        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }

        public string LicensePlate { get; set; }
        public string VehicleVIN { get; set; }

        // Money
        public decimal EstimatedAmount { get; set; }
        public decimal Cost { get; set; }
        public decimal PaidAmount { get; set; }
        public PaidStatus PaidStatus { get; set; }

        // Aggregates
        public int JobCount { get; set; }
        public int PartCount { get; set; } // distinct part ids across jobs
        public decimal JobsTotalAmount { get; set; } // sum of job totals

        // sample of services names (up to 3)
        public List<string> TopServiceNames { get; set; } = new();
    }
}
