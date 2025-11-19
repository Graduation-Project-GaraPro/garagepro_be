using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Authentication;
using Dtos.RoBoard;
using DataAccessLayer; // Add this for MyAppDbContext

namespace Repositories
{
    public interface IRepairOrderRepository
    {
        // Add Context property
        MyAppDbContext Context { get; }
        Task<RepairOrder?> GetRepairOrderForPaymentAsync(Guid repairOrderId, string userId);
        // Basic CRUD operations
        Task<RepairOrder?> GetByIdAsync(Guid repairOrderId);
        Task<RepairOrder> CreateAsync(RepairOrder repairOrder);
        Task<RepairOrder> UpdateAsync(RepairOrder repairOrder);
        Task<bool> DeleteAsync(Guid repairOrderId);
        Task<bool> ExistsAsync(Guid repairOrderId);
        Task<int> CountAsync(Expression<Func<RepairOrder, bool>> predicate);

        // Kanban Board specific queries
        Task<IEnumerable<RepairOrder>> GetRepairOrdersByStatusAsync(int statusId);
        Task<IEnumerable<RepairOrder>> GetRepairOrdersForKanbanAsync(RoBoardFiltersDto filters = null);
        Task<RepairOrder?> GetRepairOrderWithFullDetailsAsync(Guid repairOrderId);
        Task<IEnumerable<RepairOrder>> GetAllRepairOrdersWithFullDetailsAsync();

        // Status update operations for drag-drop
        Task<bool> UpdateRepairOrderStatusAsync(Guid repairOrderId, int newStatusId, string changeNote = null);
        Task<IEnumerable<RepairOrder>> BatchUpdateStatusAsync(List<UpdateRoBoardStatusDto> updates);

        // List view specific queries with optimized projections
        Task<(IEnumerable<RepairOrder> Items, int TotalCount)> GetRepairOrdersForListViewAsync(
            RoBoardFiltersDto filters = null,
            string sortBy = "ReceiveDate",
            string sortOrder = "Desc",
            int page = 1,
            int pageSize = 50);

        // Statistics and aggregations
        Task<Dictionary<int, int>> GetRepairOrderCountsByStatusAsync(List<int> statusIds = null);
        Task<Dictionary<string, object>> GetKanbanStatisticsAsync(RoBoardFiltersDto filters = null);

        // Validation and business rules
        Task<bool> CanMoveToStatusAsync(Guid repairOrderId, int newStatusId);
        Task<IEnumerable<Label>> GetAvailableLabelsForStatusAsync(int statusId);

        // User/Customer specific queries (User-centric architecture)
        Task<IEnumerable<RepairOrder>> GetRepairOrdersByUserIdAsync(string userId);
        Task<IEnumerable<RepairOrder>> GetRepairOrdersByBranchIdAsync(Guid branchId);

        // Advanced filtering and search
        Task<IEnumerable<RepairOrder>> SearchRepairOrdersAsync(
            string searchText,
            List<int> statusIds = null,
            List<Guid> branchIds = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        // Performance optimized queries for specific DTOs
        Task<IEnumerable<RepairOrder>> GetRepairOrdersWithNavigationPropertiesAsync(
            Expression<Func<RepairOrder, bool>> predicate = null,
            params Expression<Func<RepairOrder, object>>[] includes);

        // Audit and history
        Task<IEnumerable<RepairOrder>> GetRecentlyUpdatedRepairOrdersAsync(int hours = 24);
        Task<DateTime?> GetLastStatusChangeAsync(Guid repairOrderId);

        // Archive management
        Task<bool> ArchiveRepairOrderAsync(Guid repairOrderId, string reason, string archivedByUserId);
        Task<bool> RestoreRepairOrderAsync(Guid repairOrderId, string reason, string restoredByUserId);
        Task<IEnumerable<RepairOrder>> GetArchivedRepairOrdersAsync(RoBoardFiltersDto filters = null);
        Task<IEnumerable<RepairOrder>> GetActiveRepairOrdersAsync(RoBoardFiltersDto filters = null);
        Task<bool> IsRepairOrderArchivedAsync(Guid repairOrderId);

        // Archive Support Methods
        Task<RepairOrder?> GetRepairOrderByIdAsync(Guid repairOrderId);
        Task<RepairOrder> AddAsync(RepairOrder repairOrder);
        Task<IEnumerable<RepairOrder>> GetOrdersForAutoArchiveAsync(int statusId, DateTime cutoffDate);
        Task<bool> RestoreArchivedRepairOrderAsync(Guid repairOrderId);
        Task<bool> UpdateCompletionStatusAsync(Guid repairOrderId, bool isVehiclePickedUp, DateTime? vehiclePickupDate, bool isFullyPaid, DateTime? fullPaymentDate, string? notes = null);
    }
}