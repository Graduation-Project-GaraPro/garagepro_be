using BusinessObject;
using BusinessObject.Enums;
using Dtos.Branches;
using Dtos.Job;
using Dtos.RepairOrder;
using Dtos.Revenue;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Services
{
    public class RevenueService : IRevenueService
    {
        private readonly IRepairOrderRepository _repairOrderRepository;

        public RevenueService(IRepairOrderRepository repairOrderRepository)
        {
            _repairOrderRepository = repairOrderRepository;
        }

        public async Task<RevenueReportDto> GetRevenueReportAsync(RevenueFiltersDto filters)
        {
            // --- Lấy danh sách đơn hàng đầy đủ chi tiết ---
            var orders = await _repairOrderRepository.GetAllRepairOrdersWithFullDetailsAsync();

            // --- Lọc theo các tiêu chí ---
            if (filters.BranchId != null && filters.BranchId != Guid.Empty)
                orders = orders.Where(o => o.BranchId == filters.BranchId);

            // --- Xác định khoảng thời gian ---
            var startDate = filters.StartDate ?? DateTime.Now.AddDays(-30);
            var endDate = filters.EndDate ?? DateTime.Now;

            var currentPeriodOrders = orders
                .Where(o => o.CompletionDate != null &&
                            o.CompletionDate >= startDate &&
                            o.CompletionDate <= endDate)
                .ToList();

            // --- Tính toán chính ---
            var totalRevenue = currentPeriodOrders.Sum(o => o.PaidAmount);
            var totalOrders = currentPeriodOrders.Count;
            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            // --- Kỳ trước ---
            var periodLength = endDate - startDate;
            var previousStart = startDate - periodLength;
            var previousEnd = startDate;

            var previousOrders = orders
                .Where(o => o.CompletionDate != null &&
                            o.CompletionDate >= previousStart &&
                            o.CompletionDate <= previousEnd)
                .ToList();

            var previousRevenue = previousOrders.Sum(o => o.PaidAmount);
            var growthRate = previousRevenue == 0
                ? 100
                : ((totalRevenue - previousRevenue) / previousRevenue) * 100;

            // --- 🔥 Tính top dịch vụ ---
            var serviceJobs = currentPeriodOrders
                .Where(o => o.Jobs != null && o.Jobs.Any())
                .SelectMany(o => o.Jobs
                    .Where(j => j.Service != null)
                    .Select(j => new
                    {
                        j.Service.ServiceId,
                        j.Service.ServiceName,
                        j.TotalAmount,
                        o.RepairOrderId
                    }))
                .ToList();

            var totalRevenueService = serviceJobs.Sum(j => j.TotalAmount);

            var topServices = serviceJobs
                .GroupBy(j => new { j.ServiceId, j.ServiceName })
                .Select(g => new TopServiceDto
                {
                    ServiceName = g.Key.ServiceName,
                    Revenue = g.Sum(x => x.TotalAmount),
                    OrderCount = g.Select(x => x.RepairOrderId).Distinct().Count(),
                    PercentageOfTotal = totalRevenueService > 0
                        ? Math.Round((double)(g.Sum(x => x.TotalAmount) / totalRevenueService) * 100, 2)
                        : 0
                })
                .OrderByDescending(s => s.Revenue)
                .Take(5)
                .ToList();

            // --- 📊 Xu hướng dịch vụ theo tuần ---
            var jobData = orders
                .Where(o => o.Jobs != null)
                .SelectMany(o => o.Jobs
                    .Where(j => j.Service != null)
                    .Select(j => new
                    {
                        o.CompletionDate,
                        j.Service.ServiceName,
                        j.TotalAmount
                    }))
                .ToList();

            var totalRevenueTrend = jobData.Sum(j => j.TotalAmount);

            var serviceTrends = jobData
                .GroupBy(j =>
                    CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                        j.CompletionDate ?? DateTime.Now,
                        CalendarWeekRule.FirstFourDayWeek,
                        DayOfWeek.Monday))
                .Select(g =>
                {
                    var period = $"Week {g.Key}";
                    var serviceRevenue = g
                        .GroupBy(x => x.ServiceName)
                        .ToDictionary(
                            sg => sg.Key,
                            sg => sg.Sum(x => x.TotalAmount)
                        );

                    return new ServiceTrendDto
                    {
                        Period = period,
                        Services = serviceRevenue
                    };
                })
                .OrderBy(x => x.Period)
                .ToList();

            // --- Tổng hợp kết quả ---
            var report = new RevenueReportDto
            {
                Period = filters.Period,
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageOrderValue = Math.Round(avgOrderValue, 2),
                GrowthRate = Math.Round(growthRate, 2),
                PreviousPeriodRevenue = previousRevenue,
                TopServices = topServices,
                ServiceTrends = serviceTrends
            };

            return report;
        }
    }
}
