using Dtos.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Revenue
{
    public class RevenueReportDto
    {
        public string Period { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<TopServiceDto> TopServices { get; set; } = new();
        //public List<TechnicianRevenueDto> RevenueByTechnician { get; set; } = new();
        //public List<BranchRevenueDto> BranchComparison { get; set; } = new();
        public decimal GrowthRate { get; set; }
        public decimal PreviousPeriodRevenue { get; set; }

        // Optional details
        //public List<ServiceDetailDto>? DetailedServices { get; set; }
        public List<ServiceCategoryDto>? ServiceCategories { get; set; }
        public List<ServiceTrendDto>? ServiceTrends { get; set; }
        //public List<RepairOrder>? RepairOrders { get; set; }
        //public List<OrderStatusStatDto>? OrderStatusStats { get; set; }
        //public List<OrderValueDistributionDto>? OrderValueDistribution { get; set; }
    }
}
