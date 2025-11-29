using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Campaigns
{
    public class CampaignAnalyticsDto
    {
        public int TotalUsage { get; set; }

        public List<TopCustomerDto> TopCustomers { get; set; } = new();

        public List<UsageByDateDto> UsageByDate { get; set; } = new();

        public List<ServicePerformanceDto> ServicePerformance { get; set; } = new();
    }

    public class TopCustomerDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
    }

    public class UsageByDateDto
    {
        public DateTime Date { get; set; }
        public int UsageCount { get; set; }
    }

    public class ServicePerformanceDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
    }
}
