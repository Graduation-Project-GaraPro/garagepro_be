using Dtos.Revenue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IRevenueService
    {
        Task<RevenueReportDto> GetRevenueReportAsync(RevenueFiltersDto filters);
    }
}
