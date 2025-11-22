using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dtos.RepairOrderArchivedDtos;
using Dtos.RepairProgressDto;

namespace Repositories.RepairProgressRepositories
{
    public interface IRepairProgressRepository
    {
        Task<PagedResult<RepairOrderListItemDto>> GetRepairOrdersByUserIdAsync(string userId, RepairOrderFilterDto filter);
        Task<RepairOrderProgressDto?> GetRepairOrderProgressAsync(Guid repairOrderId, string userId);

        Task<PagedResult<RepairOrderArchivedListItemDto>> GetArchivedRepairOrdersByUserIdAsync(
            string userId,
            RepairOrderFilterDto filter,
            IMapper mapper);

        Task<RepairOrderArchivedDetailDto?> GetArchivedRepairOrderDetailAsync(
            Guid repairOrderId,
            string userId,
            IMapper mapper);

        Task<bool> IsRepairOrderAccessibleByUserAsync(Guid repairOrderId, string userId);
    }
}
