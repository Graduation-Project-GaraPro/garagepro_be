using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Authentication;
using DataAccessLayer;
using Dtos.RoBoard;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class RepairOrderRepository : IRepairOrderRepository
    {
        private readonly MyAppDbContext _context;

        public RepairOrderRepository(MyAppDbContext context)
        {
            _context = context;
        }

        #region Basic CRUD Operations

        public async Task<RepairOrder?> GetByIdAsync(Guid repairOrderId)
        {
            return await _context.RepairOrders
                .Include(ro => ro.OrderStatus)
                .Include(ro => ro.Branch)
                .Include(ro => ro.Vehicle)
                .Include(ro => ro.User)
                .FirstOrDefaultAsync(ro => ro.RepairOrderId == repairOrderId);
        }

        public async Task<RepairOrder> CreateAsync(RepairOrder repairOrder)
        {
            _context.RepairOrders.Add(repairOrder);
            await _context.SaveChangesAsync();
            return repairOrder;
        }

        public async Task<RepairOrder> UpdateAsync(RepairOrder repairOrder)
        {
            _context.RepairOrders.Update(repairOrder);
            await _context.SaveChangesAsync();
            return repairOrder;
        }

        public async Task<bool> DeleteAsync(Guid repairOrderId)
        {
            var repairOrder = await _context.RepairOrders.FindAsync(repairOrderId);
            if (repairOrder == null) return false;

            _context.RepairOrders.Remove(repairOrder);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid repairOrderId)
        {
            return await _context.RepairOrders.AnyAsync(ro => ro.RepairOrderId == repairOrderId);
        }

        #endregion

        #region Kanban Board Specific Queries

        public async Task<IEnumerable<RepairOrder>> GetRepairOrdersByStatusAsync(Guid statusId)
        {
            return await _context.RepairOrders
                .Include(ro => ro.OrderStatus)
                    .ThenInclude(os => os.Labels)
                        .ThenInclude(l => l.Color)
                .Include(ro => ro.Branch)
                .Include(ro => ro.Vehicle)
                .Include(ro => ro.User)
                .Where(ro => ro.StatusId == statusId && !ro.IsArchived) // Filter out archived orders
                .OrderBy(ro => ro.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<RepairOrder>> GetRepairOrdersForKanbanAsync(RoBoardFiltersDto? filters = null)
        {
            var query = _context.RepairOrders
                .Include(ro => ro.OrderStatus)
                    .ThenInclude(os => os.Labels)
                        .ThenInclude(l => l.Color)
                .Include(ro => ro.Branch)
                .Include(ro => ro.Vehicle)
                .Include(ro => ro.User)
                .Where(ro => !ro.IsArchived) // Filter out archived orders
                .AsQueryable();

            if (filters != null)
            {
                query = ApplyFilters(query, filters);
            }

            return await query
                .OrderBy(ro => ro.StatusId)
                .ThenBy(ro => ro.CreatedAt)
                .ToListAsync();
        }

        public async Task<RepairOrder?> GetRepairOrderWithFullDetailsAsync(Guid repairOrderId)
        {
            return await _context.RepairOrders
                .Include(ro => ro.OrderStatus)
                    .ThenInclude(os => os.Labels)
                        .ThenInclude(l => l.Color)
                .Include(ro => ro.Branch)
                .Include(ro => ro.Vehicle)
                .Include(ro => ro.User)
                .Include(ro => ro.RepairOrderServices)
                    .ThenInclude(ros => ros.Service)
                .Include(ro => ro.Inspections)
                .Include(ro => ro.Jobs)
                .Include(ro => ro.Payments)
                .FirstOrDefaultAsync(ro => ro.RepairOrderId == repairOrderId);
        }

        #endregion

        #region Status Update Operations

        public async Task<bool> UpdateRepairOrderStatusAsync(Guid repairOrderId, Guid newStatusId, string? changeNote = null)
        {
            var repairOrder = await _context.RepairOrders.FindAsync(repairOrderId);
            if (repairOrder == null) return false;

            var oldStatusId = repairOrder.StatusId;
            repairOrder.StatusId = newStatusId;

            // Add change note to the repair order notes if provided
            if (!string.IsNullOrEmpty(changeNote))
            {
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                var statusChangeNote = $"[{timestamp}] Status changed: {changeNote}";
                repairOrder.Note = string.IsNullOrEmpty(repairOrder.Note)
                    ? statusChangeNote
                    : $"{repairOrder.Note}\n{statusChangeNote}";
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<RepairOrder>> BatchUpdateStatusAsync(List<UpdateRoBoardStatusDto> updates)
        {
            if (updates == null || !updates.Any()) return new List<RepairOrder>();

            var repairOrderIds = updates.Select(u => u.RepairOrderId).ToList();
            var repairOrders = await _context.RepairOrders
                .Where(ro => repairOrderIds.Contains(ro.RepairOrderId))
                .ToListAsync();

            var updatedOrders = new List<RepairOrder>();

            foreach (var update in updates)
            {
                var repairOrder = repairOrders.FirstOrDefault(ro => ro.RepairOrderId == update.RepairOrderId);
                if (repairOrder != null)
                {
                    repairOrder.StatusId = update.NewStatusId;

                    if (!string.IsNullOrEmpty(update.ChangeNote))
                    {
                        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                        var statusChangeNote = $"[{timestamp}] Batch update: {update.ChangeNote}";
                        repairOrder.Note = string.IsNullOrEmpty(repairOrder.Note)
                            ? statusChangeNote
                            : $"{repairOrder.Note}\n{statusChangeNote}";
                    }

                    updatedOrders.Add(repairOrder);
                }
            }

            if (updatedOrders.Any())
            {
                await _context.SaveChangesAsync();
            }

            return updatedOrders;
        }

        #endregion

        #region List View Specific Queries

        public async Task<(IEnumerable<RepairOrder> Items, int TotalCount)> GetRepairOrdersForListViewAsync(
            RoBoardFiltersDto? filters = null,
            string sortBy = "ReceiveDate",
            string sortOrder = "Desc",
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.RepairOrders
                .Include(ro => ro.OrderStatus)
                    .ThenInclude(os => os.Labels)
                        .ThenInclude(l => l.Color)
                .Include(ro => ro.Branch)
                .Include(ro => ro.Vehicle)
                .Include(ro => ro.User)
                .Where(ro => !ro.IsArchived) // Filter out archived orders
                .AsQueryable();

            if (filters != null)
            {
                query = ApplyFilters(query, filters);
            }

            // Apply sorting
            query = ApplySorting(query, sortBy, sortOrder);

            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        #endregion

        #region Statistics and Aggregations

        public async Task<Dictionary<Guid, int>> GetRepairOrderCountsByStatusAsync(List<Guid>? statusIds = null)
        {
            var query = _context.RepairOrders.AsQueryable();

            if (statusIds != null && statusIds.Any())
            {
                query = query.Where(ro => statusIds.Contains(ro.StatusId));
            }

            return await query
                .GroupBy(ro => ro.StatusId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, object>> GetKanbanStatisticsAsync(RoBoardFiltersDto? filters = null)
        {
            var query = _context.RepairOrders.AsQueryable();

            if (filters != null)
            {
                query = ApplyFilters(query, filters);
            }

            var totalOrders = await query.CountAsync();
            var completedOrders = await query.Where(ro => ro.CompletionDate.HasValue).CountAsync();
            var overdueOrders = await query
                .Where(ro => ro.EstimatedCompletionDate.HasValue &&
                           ro.EstimatedCompletionDate.Value < DateTime.UtcNow &&
                           !ro.CompletionDate.HasValue)
                .CountAsync();

            // Handle potential empty results
            var totalRevenue = await query.SumAsync(ro => (decimal?)ro.PaidAmount) ?? 0;
            var totalEstimated = await query.SumAsync(ro => (decimal?)ro.EstimatedAmount) ?? 0;
            var pendingPayments = totalEstimated - totalRevenue;

            return new Dictionary<string, object>
            {
                ["TotalRepairOrders"] = totalOrders,
                ["CompletedOrders"] = completedOrders,
                ["OverdueOrders"] = overdueOrders,
                ["OrdersInProgress"] = totalOrders - completedOrders,
                ["TotalRevenue"] = totalRevenue,
                ["PendingPayments"] = Math.Max(0, pendingPayments) // Ensure non-negative
            };
        }

        #endregion

        #region Validation and Business Rules

        public async Task<bool> CanMoveToStatusAsync(Guid repairOrderId, Guid newStatusId)
        {
            var repairOrder = await _context.RepairOrders
                .Include(ro => ro.OrderStatus)
                .FirstOrDefaultAsync(ro => ro.RepairOrderId == repairOrderId);

            if (repairOrder == null) return false;

            var newStatus = await _context.OrderStatuses.FindAsync(newStatusId);
            if (newStatus == null) return false;

            // If it's the same status, allow the move
            if (repairOrder.StatusId == newStatusId) return true;

            // Business rules validation
            // Example: Can't move to "Completed" if there are unpaid amounts
            if (newStatus.StatusName?.ToLower().Contains("completed") == true)
            {
                if (repairOrder.PaidAmount < repairOrder.EstimatedAmount)
                {
                    return false; // Cannot complete with outstanding payments
                }
            }

            // Add more business rules as needed
            // Example: Can't move from "Completed" back to "In Progress"
            // Example: Require certain inspections before moving to specific statuses

            return true;
        }

        public async Task<IEnumerable<Label>> GetAvailableLabelsForStatusAsync(Guid statusId)
        {
            return await _context.Labels
                .Include(l => l.Color)
                .Where(l => l.OrderStatusId == statusId)
                .ToListAsync();
        }

        #endregion

        #region User/Customer Specific Queries

        public async Task<IEnumerable<RepairOrder>> GetRepairOrdersByUserIdAsync(string userId)
        {
            return await _context.RepairOrders
                .Include(ro => ro.OrderStatus)
                .Include(ro => ro.Branch)
                .Include(ro => ro.Vehicle)
                .Include(ro => ro.User)
                .Where(ro => ro.UserId == userId)
                .OrderByDescending(ro => ro.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<RepairOrder>> GetRepairOrdersByBranchIdAsync(Guid branchId)
        {
            return await _context.RepairOrders
                .Include(ro => ro.OrderStatus)
                .Include(ro => ro.Branch)
                .Include(ro => ro.Vehicle)
                .Include(ro => ro.User)
                .Where(ro => ro.BranchId == branchId)
                .OrderByDescending(ro => ro.CreatedAt)
                .ToListAsync();
        }

        #endregion

        #region Advanced Filtering and Search

        public async Task<IEnumerable<RepairOrder>> SearchRepairOrdersAsync(
            string searchText,
            List<Guid>? statusIds = null,
            List<Guid>? branchIds = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.RepairOrders
                .Include(ro => ro.OrderStatus)
                .Include(ro => ro.Branch)
                .Include(ro => ro.Vehicle)
                .Include(ro => ro.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchText))
            {
                var searchLower = searchText.ToLower();
                query = query.Where(ro =>
                    (ro.RepairOrderType != null && ro.RepairOrderType.ToLower().Contains(searchLower)) ||
                    (ro.Vehicle.LicensePlate != null && ro.Vehicle.LicensePlate.ToLower().Contains(searchLower)) ||
                    (ro.Vehicle.VIN != null && ro.Vehicle.VIN.ToLower().Contains(searchLower)) ||
                    (ro.User.FullName != null && ro.User.FullName.ToLower().Contains(searchLower)) ||
                    (ro.User.Email != null && ro.User.Email.ToLower().Contains(searchLower)) ||
                    (ro.Note != null && ro.Note.ToLower().Contains(searchLower)));
            }

            if (statusIds != null && statusIds.Any())
            {
                query = query.Where(ro => statusIds.Contains(ro.StatusId));
            }

            if (branchIds != null && branchIds.Any())
            {
                query = query.Where(ro => branchIds.Contains(ro.BranchId));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(ro => ro.ReceiveDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(ro => ro.ReceiveDate <= toDate.Value);
            }

            return await query
                .OrderByDescending(ro => ro.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<RepairOrder>> GetRepairOrdersWithNavigationPropertiesAsync(
            Expression<Func<RepairOrder, bool>>? predicate = null,
            params Expression<Func<RepairOrder, object>>[] includes)
        {
            var query = _context.RepairOrders.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }

        #endregion

        #region Audit and History

        public async Task<IEnumerable<RepairOrder>> GetRecentlyUpdatedRepairOrdersAsync(int hours = 24)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hours);

            return await _context.RepairOrders
                .Include(ro => ro.OrderStatus)
                .Include(ro => ro.Branch)
                .Include(ro => ro.Vehicle)
                .Include(ro => ro.User)
                .Where(ro => ro.CreatedAt >= cutoffTime)
                .OrderByDescending(ro => ro.CreatedAt)
                .ToListAsync();
        }

        public async Task<DateTime?> GetLastStatusChangeAsync(Guid repairOrderId)
        {
            var repairOrder = await _context.RepairOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(ro => ro.RepairOrderId == repairOrderId);

            return repairOrder?.CreatedAt; // In a real implementation, you'd track status change history
        }

        #endregion

        #region Archive Management

        public async Task<bool> ArchiveRepairOrderAsync(Guid repairOrderId, string reason, string archivedByUserId)
        {
            var repairOrder = await _context.RepairOrders.FindAsync(repairOrderId);
            if (repairOrder == null || repairOrder.IsArchived) return false;

            repairOrder.IsArchived = true;
            repairOrder.ArchivedAt = DateTime.UtcNow;
            //repairOrder.ArchiveReason = reason;
            repairOrder.ArchivedByUserId = archivedByUserId;
            repairOrder.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RestoreRepairOrderAsync(Guid repairOrderId, string reason, string restoredByUserId)
        {
            var repairOrder = await _context.RepairOrders.FindAsync(repairOrderId);
            if (repairOrder == null || !repairOrder.IsArchived) return false;

            repairOrder.IsArchived = false;
            repairOrder.ArchivedAt = null;
            //repairOrder.ArchiveReason = null;
            repairOrder.ArchivedByUserId = null;
            repairOrder.UpdatedAt = DateTime.UtcNow;

            // Add restore note to repair order
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            var restoreNote = $"[{timestamp}] Restored by {restoredByUserId}: {reason}";
            repairOrder.Note = string.IsNullOrEmpty(repairOrder.Note)
                ? restoreNote
                : $"{repairOrder.Note}\n{restoreNote}";

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<RepairOrder>> GetArchivedRepairOrdersAsync(RoBoardFiltersDto filters = null)
        {
            var query = _context.RepairOrders
                .Include(ro => ro.OrderStatus)
                    .ThenInclude(os => os.Labels)
                        .ThenInclude(l => l.Color)
                .Include(ro => ro.Branch)
                .Include(ro => ro.Vehicle)
                .Include(ro => ro.User)
                .Where(ro => ro.IsArchived)
                .AsQueryable();

            if (filters != null)
            {
                query = ApplyArchiveFilters(query, filters);
            }

            return await query
                .OrderByDescending(ro => ro.ArchivedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<RepairOrder>> GetActiveRepairOrdersAsync(RoBoardFiltersDto filters = null)
        {
            var query = _context.RepairOrders
                .Include(ro => ro.OrderStatus)
                    .ThenInclude(os => os.Labels)
                        .ThenInclude(l => l.Color)
                .Include(ro => ro.Branch)
                .Include(ro => ro.Vehicle)
                .Include(ro => ro.User)
                .Where(ro => !ro.IsArchived)
                .AsQueryable();

            if (filters != null)
            {
                query = ApplyFilters(query, filters);
            }

            return await query
                .OrderBy(ro => ro.StatusId)
                .ThenBy(ro => ro.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsRepairOrderArchivedAsync(Guid repairOrderId)
        {
            var repairOrder = await _context.RepairOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(ro => ro.RepairOrderId == repairOrderId);

            return repairOrder?.IsArchived ?? false;
        }

        #endregion

        #region Private Helper Methods

        private IQueryable<RepairOrder> ApplyFilters(IQueryable<RepairOrder> query, RoBoardFiltersDto filters)
        {
            // Archive filtering logic
            if (filters.OnlyArchived.HasValue && filters.OnlyArchived.Value)
            {
                query = query.Where(ro => ro.IsArchived);
            }
            else if (filters.IncludeArchived.HasValue && !filters.IncludeArchived.Value)
            {
                query = query.Where(ro => !ro.IsArchived);
            }
            else if (!filters.IncludeArchived.HasValue || !filters.IncludeArchived.Value)
            {
                // Default: exclude archived orders
                query = query.Where(ro => !ro.IsArchived);
            }

            if (filters.StatusIds?.Any() == true)
            {
                query = query.Where(ro => filters.StatusIds.Contains(ro.StatusId));
            }

            if (filters.CustomerIds?.Any() == true)
            {
                query = query.Where(ro => filters.CustomerIds.Contains(ro.UserId));
            }

            if (filters.FromDate.HasValue)
            {
                query = query.Where(ro => ro.ReceiveDate >= filters.FromDate.Value);
            }

            if (filters.ToDate.HasValue)
            {
                query = query.Where(ro => ro.ReceiveDate <= filters.ToDate.Value);
            }

            if (filters.MinAmount.HasValue)
            {
                query = query.Where(ro => ro.EstimatedAmount >= filters.MinAmount.Value);
            }

            if (filters.MaxAmount.HasValue)
            {
                query = query.Where(ro => ro.EstimatedAmount <= filters.MaxAmount.Value);
            }

            if (filters.IsOverdue.HasValue && filters.IsOverdue.Value)
            {
                query = query.Where(ro => ro.EstimatedCompletionDate.HasValue &&
                                         ro.EstimatedCompletionDate.Value < DateTime.UtcNow &&
                                         !ro.CompletionDate.HasValue);
            }

            if (filters.PaidStatuses?.Any() == true)
            {
                query = query.Where(ro => filters.PaidStatuses.Contains(ro.PaidStatus));
            }

            if (!string.IsNullOrEmpty(filters.RepairOrderType))
            {
                query = query.Where(ro => ro.RepairOrderType == filters.RepairOrderType);
            }

            // Apply common filters
            return ApplyCommonFilters(query, filters);
        }

        private IQueryable<RepairOrder> ApplyArchiveFilters(IQueryable<RepairOrder> query, RoBoardFiltersDto filters)
        {
            if (filters.ArchivedFromDate.HasValue)
            {
                query = query.Where(ro => ro.ArchivedAt >= filters.ArchivedFromDate.Value);
            }

            if (filters.ArchivedToDate.HasValue)
            {
                query = query.Where(ro => ro.ArchivedAt <= filters.ArchivedToDate.Value);
            }

            // Apply common filters
            return ApplyCommonFilters(query, filters);
        }

        private IQueryable<RepairOrder> ApplyCommonFilters(IQueryable<RepairOrder> query, RoBoardFiltersDto filters)
        {
            if (filters.BranchIds?.Any() == true)
            {
                query = query.Where(ro => filters.BranchIds.Contains(ro.BranchId));
            }

            if (!string.IsNullOrEmpty(filters.SearchText))
            {
                var searchLower = filters.SearchText.ToLower();
                query = query.Where(ro =>
                    (ro.RepairOrderType != null && ro.RepairOrderType.ToLower().Contains(searchLower)) ||
                    (ro.Vehicle.LicensePlate != null && ro.Vehicle.LicensePlate.ToLower().Contains(searchLower)) ||
                    (ro.User.FullName != null && ro.User.FullName.ToLower().Contains(searchLower)) ||
                    (ro.Note != null && ro.Note.ToLower().Contains(searchLower)));
            }

            return query;
        }

        private IQueryable<RepairOrder> ApplySorting(IQueryable<RepairOrder> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "receivedate" => isDescending ? query.OrderByDescending(ro => ro.ReceiveDate) : query.OrderBy(ro => ro.ReceiveDate),
                "estimatedcompletiondate" => isDescending ? query.OrderByDescending(ro => ro.EstimatedCompletionDate) : query.OrderBy(ro => ro.EstimatedCompletionDate),
                "completiondate" => isDescending ? query.OrderByDescending(ro => ro.CompletionDate) : query.OrderBy(ro => ro.CompletionDate),
                "estimatedamount" => isDescending ? query.OrderByDescending(ro => ro.EstimatedAmount) : query.OrderBy(ro => ro.EstimatedAmount),
                "paidamount" => isDescending ? query.OrderByDescending(ro => ro.PaidAmount) : query.OrderBy(ro => ro.PaidAmount),
                "customername" => isDescending ? query.OrderByDescending(ro => ro.User.FullName) : query.OrderBy(ro => ro.User.FullName),
                "statusname" => isDescending ? query.OrderByDescending(ro => ro.OrderStatus.StatusName) : query.OrderBy(ro => ro.OrderStatus.StatusName),
                "repairordertype" => isDescending ? query.OrderByDescending(ro => ro.RepairOrderType) : query.OrderBy(ro => ro.RepairOrderType),
                "vehiclelicenseplate" => isDescending ? query.OrderByDescending(ro => ro.Vehicle.LicensePlate) : query.OrderBy(ro => ro.Vehicle.LicensePlate),
                "createdat" => isDescending ? query.OrderByDescending(ro => ro.CreatedAt) : query.OrderBy(ro => ro.CreatedAt),
                _ => isDescending ? query.OrderByDescending(ro => ro.ReceiveDate) : query.OrderBy(ro => ro.ReceiveDate)
            };
        }

        #endregion

        #region Archive Support Methods

        public async Task<RepairOrder?> GetRepairOrderByIdAsync(Guid repairOrderId)
        {
            return await GetRepairOrderWithFullDetailsAsync(repairOrderId);
        }

        public async Task<RepairOrder> AddAsync(RepairOrder repairOrder)
        {
            return await CreateAsync(repairOrder);
        }

        public async Task<IEnumerable<RepairOrder>> GetOrdersForAutoArchiveAsync(Guid statusId, DateTime cutoffDate)
        {
            return await _context.RepairOrders
                .Include(ro => ro.OrderStatus)
                .Include(ro => ro.Vehicle)
                .Include(ro => ro.User)
                .Include(ro => ro.Branch)
                .Where(ro => ro.StatusId == statusId &&
                           !ro.IsArchived &&
                           (ro.CompletionDate.HasValue ? ro.CompletionDate.Value <= cutoffDate : ro.UpdatedAt <= cutoffDate))
                .ToListAsync();
        }

        public async Task<bool> RestoreArchivedRepairOrderAsync(Guid repairOrderId)
        {
            var repairOrder = await _context.RepairOrders.FindAsync(repairOrderId);
            if (repairOrder == null) return false;

            repairOrder.IsArchived = false;
            repairOrder.ArchivedAt = null;
            //repairOrder.ArchiveReason = null;
            repairOrder.ArchivedByUserId = null;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCompletionStatusAsync(Guid repairOrderId, bool isVehiclePickedUp, DateTime? vehiclePickupDate, bool isFullyPaid, DateTime? fullPaymentDate, string? notes = null)
        {
            var repairOrder = await _context.RepairOrders.FindAsync(repairOrderId);
            if (repairOrder == null) return false;

            // Update completion status based on parameters
            if (isVehiclePickedUp && vehiclePickupDate.HasValue)
            {
                repairOrder.CompletionDate = vehiclePickupDate;
            }

            if (isFullyPaid && fullPaymentDate.HasValue)
            {
                repairOrder.PaidStatus = "Paid";
                // Update payment amount to estimated amount if fully paid
                repairOrder.PaidAmount = repairOrder.EstimatedAmount;
            }

            if (!string.IsNullOrEmpty(notes))
            {
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                var completionNote = $"[{timestamp}] Completion update: {notes}";
                repairOrder.Note = string.IsNullOrEmpty(repairOrder.Note)
                    ? completionNote
                    : $"{repairOrder.Note}\n{completionNote}";
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}