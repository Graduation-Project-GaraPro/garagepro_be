using Dtos.RepairOrder;
using Dtos.Revenue;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Garage_pro_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IRevenueService _revenueService;
        public StatisticsController(IRevenueService revenueService)
        {
            _revenueService = revenueService;
        }


        [HttpGet("revenue")]
        public async Task<ActionResult<RevenueReportDto>> GetRevenueStatistics([FromQuery] RevenueFiltersDto filters)
        {
            // Nếu người dùng không truyền ngày => mặc định 30 ngày gần nhất
            filters.StartDate ??= DateTime.Today.AddDays(-30);
            filters.EndDate ??= DateTime.Today;

            var report = await _revenueService.GetRevenueReportAsync(filters);
            return Ok(report);
        }
    }
}
