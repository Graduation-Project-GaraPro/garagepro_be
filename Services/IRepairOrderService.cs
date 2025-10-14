using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;
using Dtos.RepairOrder;
using Dtos.RoBoard;

namespace Services
{
    public interface IRepairOrderService
    {
        // Kanban Board Operations
        Task<RoBoardDto> GetKanbanBoardAsync(RoBoardFiltersDto filters = null);
        Task<RoBoardListViewDto> GetListViewAsync(RoBoardFiltersDto filters = null, string sortBy = "ReceiveDate", string sortOrder = "Desc", int page = 1, int pageSize = 50);

        // Drag & Drop Operations
        Task<RoBoardStatusUpdateResultDto> UpdateRepairOrderStatusAsync(UpdateRoBoardStatusDto updateDto);
        Task<BatchRoBoardStatusUpdateResultDto> BatchUpdateRepairOrderStatusAsync(BatchUpdateRoBoardStatusDto batchUpdateDto);
        Task<RoBoardMoveValidationDto> ValidateMoveAsync(Guid repairOrderId, Guid fromStatusId, Guid toStatusId);

        // CRUD Operations
        Task<RoBoardCardDto?> GetRepairOrderCardAsync(Guid repairOrderId);
        Task<RepairOrder> CreateRepairOrderAsync(RepairOrder repairOrder);
        Task<RepairOrder> UpdateRepairOrderAsync(RepairOrder repairOrder);
        Task<bool> DeleteRepairOrderAsync(Guid repairOrderId);
        Task<bool> RepairOrderExistsAsync(Guid repairOrderId);
        
        // NEW: Get all repair orders with OData support
        Task<IEnumerable<RepairOrder>> GetAllRepairOrdersAsync();
        
        // NEW: Get repair orders by status
        Task<IEnumerable<RepairOrder>> GetRepairOrdersByStatusAsync(Guid statusId);
        
        // NEW: Get repair order with full details
        Task<RepairOrder> GetRepairOrderWithFullDetailsAsync(Guid repairOrderId);
        
        // NEW: Map RepairOrder to RepairOrderDto
        RepairOrderDto MapToRepairOrderDto(RepairOrder repairOrder);
        
        // Statistics and Analytics
        Task<RoBoardStatisticsDto> GetBoardStatisticsAsync(RoBoardFiltersDto filters = null);
        Task<Dictionary<Guid, int>> GetRepairOrderCountsByStatusAsync(List<Guid> statusIds = null);

        // User-specific Operations
        Task<IEnumerable<RoBoardCardDto>> GetRepairOrdersByUserAsync(string userId);
        Task<IEnumerable<RoBoardCardDto>> GetRepairOrdersByBranchAsync(Guid branchId);

        // Search and Filtering
        Task<IEnumerable<RoBoardCardDto>> SearchRepairOrdersAsync(string searchText, List<Guid> statusIds = null, List<Guid> branchIds = null, DateTime? fromDate = null, DateTime? toDate = null);

        // Business Rules and Validation
        Task<bool> CanMoveToStatusAsync(Guid repairOrderId, Guid newStatusId);
        Task<IEnumerable<RoBoardLabelDto>> GetAvailableLabelsForStatusAsync(Guid statusId);

        // Audit and History
        Task<IEnumerable<RoBoardCardDto>> GetRecentlyUpdatedRepairOrdersAsync(int hours = 24);

        // Simplified 3-Status Operations (Pending, In Progress, Completed)
        Task<RoBoardColumnDto> GetPendingOrdersAsync(RoBoardFiltersDto filters = null);
        Task<RoBoardColumnDto> GetInProgressOrdersAsync(RoBoardFiltersDto filters = null);
        Task<RoBoardColumnDto> GetCompletedOrdersAsync(RoBoardFiltersDto filters = null);

        // Board Configuration
        Task<RoBoardConfigurationDto> GetBoardConfigurationAsync();
        Task<RoBoardPermissionsDto> GetUserPermissionsAsync(string userId, List<string> userRoles);

        // Archive Management Operations
        Task<ArchiveOperationResultDto> ArchiveRepairOrderAsync(ArchiveRepairOrderDto archiveDto);
        Task<ArchiveOperationResultDto> RestoreRepairOrderAsync(RestoreRepairOrderDto restoreDto);
        Task<RoBoardListViewDto> GetArchivedRepairOrdersAsync(RoBoardFiltersDto filters = null, string sortBy = "ArchivedAt", string sortOrder = "Desc", int page = 1, int pageSize = 50);
        Task<bool> IsRepairOrderArchivedAsync(Guid repairOrderId);
    }
}