using Dtos.Job;
using Dtos.RepairOrder;
using Dtos.Revenue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Revenue
{
    public interface IAdminRepairOrderRepository
    { // For list UI (projection + paging + optional date range)
        Task<List<RepairOrderListItemDto>> GetRepairOrdersForListAsync(
            DateTime? startDate = null, DateTime? endDate = null, int page = 0, int pageSize = 50);

        // Summaries for revenue aggregation (filters applied in DB)
        Task<List<RepairOrderSummaryDto>> GetRepairOrderSummariesAsync(
            DateTime start, DateTime end, Guid? branchId = null, Guid? technicianId = null, string serviceType = null);

        // Jobs used for top service / trends (filters applied in DB)
        Task<List<JobSummaryDto>> GetJobSummariesByCompletionDateRangeAsync(
            DateTime start, DateTime end, Guid? branchId = null, Guid? technicianId = null, string serviceType = null);

        // Optional helper: job summaries by repair order ids (batch)
        Task<List<JobSummaryDto>> GetJobSummariesByRepairOrderIdsAsync(IEnumerable<Guid> repairOrderIds);
    }
}
