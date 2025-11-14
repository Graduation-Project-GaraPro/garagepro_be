using BusinessObject.Enums;
using System;
using System.Collections.Generic;

namespace Dtos.RepairOrder
{
    public class RepairOrderTrendDto
    {
        public string Period { get; set; } // "2025-11-01" or "2025-11" or "2025"
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopOrderDto
    {
        public Guid RepairOrderId { get; set; }
        public string ShortId { get; set; }
        public DateTime? Date { get; set; }
        public decimal Amount { get; set; }
        public string BranchName { get; set; }
        public string CustomerName { get; set; }
        public PaidStatus PaidStatus { get; set; }
    }

    public class RepairOrdersTrendsResponse
    {
        public List<RepairOrderTrendDto> Trends { get; set; } = new();
        public List<TopOrderDto> TopOrders { get; set; } = new();
    }
}
