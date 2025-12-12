using BusinessObject.Enums;
using System;

namespace Dtos.Revenue
{
    public class RepairOrderSummaryDto
    {
        public Guid RepairOrderId { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Cost { get; set; }
        public decimal EstimatedAmount { get; set; }
        public int StatusId { get; set; }
        public Guid BranchId { get; set; }
        public PaidStatus PaidStatus { get; set; }
    }
}
