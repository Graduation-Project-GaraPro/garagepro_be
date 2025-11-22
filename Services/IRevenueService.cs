using Dtos.Revenue;
using System.Threading.Tasks;

namespace Services
{
    public interface IRevenueService
    {
        Task<RevenueReportDto> GetRevenueReportAsync(RevenueFiltersDto filters);
    }
}
