using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dtos.RepairOrderArchivedDtos;
using Dtos.RepairProgressDto;

namespace Services.RepairProgressServices
{
    public interface IRepairProgressService
    {
        Task<PagedResult<RepairOrderListItemDto>> GetUserRepairOrdersAsync(string userId, RepairOrderFilterDto filter);
        Task<RepairOrderProgressDto?> GetRepairOrderProgressAsync(Guid repairOrderId, string userId);

        Task<PagedResult<RepairOrderArchivedListItemDto>> GetArchivedRepairOrdersAsync(
        string userId,
        RepairOrderFilterDto filter);

        Task<RepairOrderArchivedDetailDto?> GetArchivedRepairOrderDetailAsync(
            Guid repairOrderId,
            string userId);
    }

}
