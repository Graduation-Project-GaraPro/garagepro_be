using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dtos.RepairProgressDto;

namespace Repositories.RepairProgressRepositories
{
    public interface IRepairProgressRepository
    {
        Task<PagedResult<RepairOrderListItemDto>> GetRepairOrdersByUserIdAsync(string userId, RepairOrderFilterDto filter);
        Task<RepairOrderProgressDto?> GetRepairOrderProgressAsync(Guid repairOrderId, string userId);
        Task<bool> IsRepairOrderAccessibleByUserAsync(Guid repairOrderId, string userId);
    }
}
