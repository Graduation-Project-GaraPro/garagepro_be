using Dtos.RepairOrder;
using Dtos.Revenue;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using Repositories.Revenue;
using Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IAdminRepairOrderRepository _repairOrderRepo;
        private readonly IRevenueService _revenueService;

        public StatisticsController(IAdminRepairOrderRepository repairOrderRepo, IRevenueService revenueService)
        {
            _repairOrderRepo = repairOrderRepo;
            _revenueService = revenueService;
        }

        // Existing endpoint...
        [HttpGet("revenue")]
        public async Task<ActionResult> GetRevenueStatistics([FromQuery] RevenueFiltersDto filters)
        {
            if (filters.StartDate.HasValue && filters.EndDate.HasValue && filters.StartDate > filters.EndDate)
                return BadRequest("StartDate must be <= EndDate");

            var report = await _revenueService.GetRevenueReportAsync(filters ?? new RevenueFiltersDto());
            return Ok(report);
        }

        // 1) List lightweight orders for UI (supports filters + paging)
        [HttpGet("repairorders")]
        public async Task<ActionResult> GetRepairOrders([FromQuery] RevenueFiltersDto filters,
            [FromQuery] int page = 0, [FromQuery] int pageSize = 50)
        {
            // derive period range if StartDate/EndDate passed; otherwise null means all
            DateTime? start = filters?.StartDate;
            DateTime? end = filters?.EndDate;
            var items = await _repairOrderRepo.GetRepairOrdersForListAsync(start, end, page, pageSize);
            return Ok(items);
        }

        // 2) Single order detail
        [HttpGet("repairorders/{id:guid}")]
        public async Task<ActionResult> GetRepairOrderDetail(Guid id)
        {
            // implement small repo method or inline projection
            // We'll implement inline projection for detail to avoid adding new repo method now
            var ro = await _repairOrderRepo.GetRepairOrdersForListAsync(null, null, 0, int.MaxValue)
                        .ContinueWith(t => t.Result.FirstOrDefault(r => r.RepairOrderId == id));
            // better to implement repo.GetRepairOrderDetailAsync(id) — but this quick fallback:
            if (ro == null) return NotFound();

            // For better detail, query DB directly
            // We'll assume repo has GetRepairOrderDetailAsync — if not, implement below in repo.
            return Ok(ro);
        }

        // 3) Trends for orders (group by daily/monthly/year depending on 'period')
        [HttpGet("repairorders/trends")]
        public async Task<ActionResult<RepairOrdersTrendsResponse>> GetRepairOrderTrends([FromQuery] RevenueFiltersDto filters)
        {
            // compute period range using RevenueService helper via calling service
            var (start, end) = CallGetPeriodRange(filters);
            // Use repo to get repair order summaries
            var roSummaries = await _repairOrderRepo.GetRepairOrderSummariesAsync(start, end, filters?.BranchId, filters?.TechnicianId, filters?.ServiceType);

            // Build trend grouping depending on filters.Period
            var period = (filters?.Period ?? "monthly").ToLower();
            var trends = period switch
            {
                "daily" => roSummaries
                    .GroupBy(r => r.CompletionDate?.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new RepairOrderTrendDto
                    {
                        Period = g.Key?.ToString("yyyy-MM-dd") ?? "Unknown",
                        TotalRevenue = g.Sum(x => x.Cost),
                        OrderCount = g.Count()
                    }).ToList(),
                "yearly" => roSummaries
                    .GroupBy(r => r.CompletionDate?.Year)
                    .OrderBy(g => g.Key)
                    .Select(g => new RepairOrderTrendDto
                    {
                        Period = g.Key?.ToString() ?? "Unknown",
                        TotalRevenue = g.Sum(x => x.Cost),
                        OrderCount = g.Count()
                    }).ToList(),
                _ => // monthly
                    roSummaries
                    .GroupBy(r => new { Year = r.CompletionDate?.Year, Month = r.CompletionDate?.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new RepairOrderTrendDto
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:00}",
                        TotalRevenue = g.Sum(x => x.Cost),
                        OrderCount = g.Count()
                    }).ToList()
            };

            // Top orders by paid amount for this period (take top 10)
            var topOrders = roSummaries
                .OrderByDescending(r => r.Cost)
                .Take(10)
                .Select(r => new TopOrderDto
                {
                    RepairOrderId = r.RepairOrderId,
                    ShortId = r.RepairOrderId.ToString().Substring(0, 8),
                    Date = r.CompletionDate,
                    Amount = r.Cost,
                    BranchName = "", // fill if you have branch query; for now blank
                    CustomerName = "", // blank unless you project it in DTO
                    PaidStatus = r.PaidStatus
                }).ToList();

            var resp = new RepairOrdersTrendsResponse
            {
                Trends = trends,
                TopOrders = topOrders
            };

            return Ok(resp);
        }

        // Helper: replicate GetPeriodRange logic (you can instead expose helper from service)
        private (DateTime start, DateTime end) CallGetPeriodRange(RevenueFiltersDto filters)
        {
            var now = DateTime.Now;
            var today = now.Date;
            var startOfDay = today;
            var endOfDay = today.AddDays(1).AddTicks(-1);

            if (filters == null) return (now.AddDays(-30).Date, endOfDay);

            return filters.Period?.ToLower() switch
            {
                "daily" => filters.StartDate.HasValue
                    ? (filters.StartDate.Value.Date, (filters.EndDate ?? filters.StartDate.Value).Date.AddDays(1).AddTicks(-1))
                    : (startOfDay, endOfDay),

                "monthly" => filters.StartDate.HasValue
                    ? (new DateTime(filters.StartDate.Value.Year, filters.StartDate.Value.Month, 1),
                       (filters.EndDate.HasValue ? new DateTime(filters.EndDate.Value.Year, filters.EndDate.Value.Month, 1).AddMonths(1).AddTicks(-1)
                                                 : new DateTime(filters.StartDate.Value.Year, filters.StartDate.Value.Month, 1).AddMonths(1).AddTicks(-1)))
                    : (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1)),

                "yearly" => filters.StartDate.HasValue
                    ? (new DateTime(filters.StartDate.Value.Year, 1, 1), new DateTime(filters.EndDate?.Year ?? filters.StartDate.Value.Year, 12, 31, 23, 59, 59))
                    : (new DateTime(now.Year, 1, 1), new DateTime(now.Year, 12, 31, 23, 59, 59)),

                _ => filters.StartDate.HasValue
                    ? (filters.StartDate.Value.Date, (filters.EndDate ?? filters.StartDate.Value).Date.AddDays(1).AddTicks(-1))
                    : (now.AddDays(-30).Date, endOfDay)
            };
        }
    }
}
