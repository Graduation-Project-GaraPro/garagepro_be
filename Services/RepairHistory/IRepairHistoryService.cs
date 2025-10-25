using Dtos.RepairHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.RepairHistory
{
    public interface IRepairHistoryService
    {
        Task<List<RepairHistoryDto>> GetRepairHistoryByUserIdAsync(string userId);
    }
}
