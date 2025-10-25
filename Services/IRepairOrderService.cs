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

        Task<RoBoardMoveValidationDto> ValidateMoveAsync(Guid repairOrderId, int fromStatusId, int toStatusId);
        
        // CRUD Operations
        Task<RoBoardCardDto?> GetRepairOrderCardAsync(Guid repairOrderId);
        Task<RepairOrder> CreateRepairOrderAsync(RepairOrder repairOrder);
        Task<RepairOrder> UpdateRepairOrderAsync(RepairOrder repairOrder);
        Task<bool> DeleteRepairOrderAsync(Guid repairOrderId);
        Task<bool> RepairOrderExistsAsync(Guid repairOrderId);

        Task<RepairOrder> GetRepairOrderWithFullDetailsAsync(Guid repairOrderId);

        // NEW: Get all repair orders with OData support
        Task<IEnumerable<RepairOrder>> GetAllRepairOrdersAsync();
        
        // NEW: Get repair orders by status
        Task<IEnumerable<RepairOrder>> GetRepairOrdersByStatusAsync(int statusId);
        
        // NEW: Map RepairOrder to RepairOrderDto
        RepairOrderDto MapToRepairOrderDto(RepairOrder repairOrder);
        
        // Statistics and Analytics
        Task<RoBoardStatisticsDto> GetBoardStatisticsAsync(RoBoardFiltersDto filters = null);
        Task<Dictionary<int, int>> GetRepairOrderCountsByStatusAsync(List<int> statusIds = null);
        

        // User-specific Operations
        Task<IEnumerable<RoBoardCardDto>> GetRepairOrdersByUserAsync(string userId);
        Task<IEnumerable<RoBoardCardDto>> GetRepairOrdersByBranchAsync(Guid branchId);

        // Search and Filtering
        Task<IEnumerable<RoBoardCardDto>> SearchRepairOrdersAsync(string searchText, List<int> statusIds = null, List<Guid> branchIds = null, DateTime? fromDate = null, DateTime? toDate = null);
        
        // Business Rules and Validation
        Task<bool> CanMoveToStatusAsync(Guid repairOrderId, int newStatusId);
        Task<IEnumerable<RoBoardLabelDto>> GetAvailableLabelsForStatusAsync(int statusId);            
        
        // Board Configuration
        Task<RoBoardConfigurationDto> GetBoardConfigurationAsync();
        
        // Archive Management Operations
        Task<ArchiveOperationResultDto> ArchiveRepairOrderAsync(ArchiveRepairOrderDto archiveDto);
        Task<ArchiveOperationResultDto> RestoreRepairOrderAsync(RestoreRepairOrderDto restoreDto);
        Task<RoBoardListViewDto> GetArchivedRepairOrdersAsync(RoBoardFiltersDto filters = null, string sortBy = "ArchivedAt", string sortOrder = "Desc", int page = 1, int pageSize = 50);
        Task<bool> IsRepairOrderArchivedAsync(Guid repairOrderId);
    }
}