using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dtos.RepairProgressDto;
using Microsoft.Extensions.Logging;
using Repositories;
using Repositories.RepairProgressRepositories;

namespace Services.RepairProgressServices
{
    public class RepairProgressService : IRepairProgressService
    {
        private readonly IRepairProgressRepository _repairProgressRepository;
        private readonly ILogger<RepairOrderService> _logger;

        public RepairProgressService(IRepairProgressRepository repairProgressRepository, ILogger<RepairOrderService> logger)
        {
            _repairProgressRepository = repairProgressRepository;
            _logger = logger;
        }

        public async Task<PagedResult<RepairOrderListItemDto>> GetUserRepairOrdersAsync(string userId, RepairOrderFilterDto filter)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                // Validate pagination parameters
                if (filter.PageNumber < 1) filter.PageNumber = 1;
                if (filter.PageSize < 1 || filter.PageSize > 100) filter.PageSize = 10;

                return await _repairProgressRepository.GetRepairOrdersByUserIdAsync(userId, filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repair orders for user {UserId}", userId);
                throw;
            }
        }

        public async Task<RepairOrderProgressDto?> GetRepairOrderProgressAsync(Guid repairOrderId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                // Check if user has access to this repair order
                var hasAccess = await _repairProgressRepository.IsRepairOrderAccessibleByUserAsync(repairOrderId, userId);
                if (!hasAccess)
                {
                    _logger.LogWarning("User {UserId} attempted to access repair order {RepairOrderId} without permission", userId, repairOrderId);
                    return null;
                }

                return await _repairProgressRepository.GetRepairOrderProgressAsync(repairOrderId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repair order progress for order {RepairOrderId} and user {UserId}", repairOrderId, userId);
                throw;
            }
        }
    }
}
