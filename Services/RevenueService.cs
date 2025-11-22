using Dtos.Revenue;
using Dtos.Job;
using Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Repositories.Revenue;

namespace Services
{
    public class RevenueService : IRevenueService
    {
        private readonly IAdminRepairOrderRepository _repairOrderRepository;

        public RevenueService(IAdminRepairOrderRepository repairOrderRepository)
        {
            _repairOrderRepository = repairOrderRepository;
        }

        public async Task<RevenueReportDto> GetRevenueReportAsync(RevenueFiltersDto filters)
        {
            // 1. Calculate period range
            var (startDate, endDate) = GetPeriodRange(filters);

            // 2. Retrieve repair order summaries with filters (branch/technician/serviceType)
            var roSummaries = await _repairOrderRepository.GetRepairOrderSummariesAsync(
                startDate, endDate, filters.BranchId, filters.TechnicianId, filters.ServiceType);

            // 3. Basic metrics
            var totalRevenue = roSummaries.Sum(r => r.PaidAmount);
            var totalOrders = roSummaries.Count;
            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0m;

            // 4. Previous period for growth
            var periodLength = endDate - startDate;
            var previousStart = startDate - periodLength;
            var previousEnd = startDate.AddTicks(-1);

            var prevSummaries = await _repairOrderRepository.GetRepairOrderSummariesAsync(
                previousStart, previousEnd, filters.BranchId, filters.TechnicianId, filters.ServiceType);

            var previousRevenue = prevSummaries.Sum(r => r.PaidAmount);
            var growthRate = previousRevenue == 0 ? 100m : ((totalRevenue - previousRevenue) / previousRevenue) * 100m;

            // 5. Top services & trends -> use job summaries (filtered)
            var jobSummaries = await _repairOrderRepository.GetJobSummariesByCompletionDateRangeAsync(
                startDate, endDate, filters.BranchId, filters.TechnicianId, filters.ServiceType);

            var totalRevenueService = jobSummaries.Sum(j => j.TotalAmount);

            var topServices = jobSummaries
                .Where(j => !string.IsNullOrEmpty(j.ServiceName))
                .GroupBy(j => new { j.ServiceId, j.ServiceName })
                .Select(g => new TopServiceDto
                {
                    ServiceName = g.Key.ServiceName,
                    Revenue = Math.Round(g.Sum(x => x.TotalAmount), 2),
                    OrderCount = g.Select(x => x.RepairOrderId).Distinct().Count(),
                    PercentageOfTotal = totalRevenueService > 0 ? Math.Round((double)(g.Sum(x => x.TotalAmount) / totalRevenueService) * 100, 2) : 0
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToList();

            // 6. Trends grouping based on period type
            var period = filters.Period?.ToLower() ?? "monthly";
            List<ServiceTrendDto> serviceTrends = new();

            if (period == "daily")
            {
                serviceTrends = jobSummaries
                    .Where(j => j.CreatedAt != default)
                    .GroupBy(j => j.CreatedAt.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new ServiceTrendDto
                    {
                        Period = g.Key.ToString("yyyy-MM-dd"),
                        Services = g.GroupBy(x => x.ServiceName).ToDictionary(gg => gg.Key ?? "Unknown", gg => gg.Sum(x => x.TotalAmount))
                    })
                    .ToList();
            }
            else if (period == "yearly")
            {
                serviceTrends = jobSummaries
                    .Where(j => j.CreatedAt != default)
                    .GroupBy(j => j.CreatedAt.Year)
                    .OrderBy(g => g.Key)
                    .Select(g => new ServiceTrendDto
                    {
                        Period = g.Key.ToString(),
                        Services = g.GroupBy(x => x.ServiceName).ToDictionary(gg => gg.Key ?? "Unknown", gg => gg.Sum(x => x.TotalAmount))
                    })
                    .ToList();
            }
            else // monthly (default)
            {
                serviceTrends = jobSummaries
                    .Where(j => j.CreatedAt != default)
                    .GroupBy(j => new { j.CreatedAt.Year, j.CreatedAt.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new ServiceTrendDto
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:00}",
                        Services = g.GroupBy(x => x.ServiceName).ToDictionary(gg => gg.Key ?? "Unknown", gg => gg.Sum(x => x.TotalAmount))
                    })
                    .ToList();
            }

            // 7. Build report
            var report = new RevenueReportDto
            {
                Period = filters.Period,
                TotalRevenue = Math.Round(totalRevenue, 2),
                TotalOrders = totalOrders,
                AverageOrderValue = Math.Round(avgOrderValue, 2),
                GrowthRate = Math.Round(growthRate, 2),
                PreviousPeriodRevenue = Math.Round(previousRevenue, 2),
                TopServices = topServices,
                ServiceTrends = serviceTrends
            };

            return report;
        }

        #region Helpers - period range

        private (DateTime start, DateTime end) GetPeriodRange(RevenueFiltersDto filters)
        {
            var now = DateTime.Now;
            var today = now.Date;
            var startOfDay = today;
            var endOfDay = today.AddDays(1).AddTicks(-1);

            return filters.Period?.ToLower() switch
            {
                "daily" => filters.StartDate.HasValue
                    ? (filters.StartDate.Value.Date, (filters.EndDate ?? filters.StartDate.Value).Date.AddDays(1).AddTicks(-1))
                    : (startOfDay, endOfDay),

                "monthly" => filters.StartDate.HasValue
                    ? GetMonthRange(filters.StartDate.Value, filters.EndDate)
                    : GetCurrentMonthRange(now),

                "yearly" => filters.StartDate.HasValue
                    ? GetYearRange(filters.StartDate.Value.Year, filters.EndDate?.Year)
                    : (new DateTime(now.Year, 1, 1), new DateTime(now.Year, 12, 31, 23, 59, 59)),

                _ => filters.StartDate.HasValue
                    ? (filters.StartDate.Value.Date, (filters.EndDate ?? filters.StartDate.Value).Date.AddDays(1).AddTicks(-1))
                    : (now.AddDays(-30).Date, endOfDay)
            };
        }

        private (DateTime start, DateTime end) GetCurrentMonthRange(DateTime now)
        {
            var firstDay = new DateTime(now.Year, now.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddTicks(-1);
            return (firstDay, lastDay);
        }

        private (DateTime start, DateTime end) GetMonthRange(DateTime start, DateTime? end)
        {
            var s = new DateTime(start.Year, start.Month, 1);
            var e = end.HasValue
                ? new DateTime(end.Value.Year, end.Value.Month, 1).AddMonths(1).AddTicks(-1)
                : s.AddMonths(1).AddTicks(-1);
            return (s, e);
        }

        private (DateTime start, DateTime end) GetYearRange(int year, int? endYear)
        {
            var start = new DateTime(year, 1, 1);
            var end = endYear.HasValue
                ? new DateTime(endYear.Value, 12, 31, 23, 59, 59)
                : new DateTime(year, 12, 31, 23, 59, 59);
            return (start, end);
        }

        #endregion
    }
}
