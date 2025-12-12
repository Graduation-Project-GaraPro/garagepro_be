using BusinessObject;
using BusinessObject.Enums;
using Dtos.RepairOrder;
using Dtos.RoBoard;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
    public interface IRepairOrderService
    {
        #region Kanban Board Operations
        Task<RoBoardDto> GetKanbanBoardAsync(RoBoardFiltersDto filters = null);
        Task<RoBoardListViewDto> GetListViewAsync(RoBoardFiltersDto filters = null, string sortBy = "ReceiveDate", string sortOrder = "Desc", int page = 1, int pageSize = 50);
        #endregion

        #region Drag & Drop Operations
        Task<RoBoardStatusUpdateResultDto> UpdateRepairOrderStatusAsync(UpdateRoBoardStatusDto updateDto);
        Task<RoBoardMoveValidationDto> ValidateMoveAsync(Guid repairOrderId, int fromStatusId, int toStatusId);
        #endregion

        #region CRUD Operations
        Task<RoBoardCardDto?> GetRepairOrderCardAsync(Guid repairOrderId);
        Task<RepairOrder> CreateRepairOrderAsync(RepairOrder repairOrder, List<Guid> selectedServiceIds = null);
        Task<RepairOrder> UpdateRepairOrderAsync(RepairOrder repairOrder);
        Task UpdateCarPickupStatusAsync(Guid repairOrderId, string userId, CarPickupStatus status);
        Task<bool> DeleteRepairOrderAsync(Guid repairOrderId);
        Task<IEnumerable<RepairOrder>> GetAllRepairOrdersAsync();
        Task<IEnumerable<RepairOrder>> GetRepairOrdersByStatusAsync(int statusId);
        Task<RepairOrder> GetRepairOrderWithFullDetailsAsync(Guid repairOrderId);
        Task<bool> RepairOrderExistsAsync(Guid repairOrderId);
        #endregion

        #region User-specific Operations
        Task<IEnumerable<RoBoardCardDto>> GetRepairOrdersByUserAsync(string userId);
        Task<IEnumerable<RoBoardCardDto>> GetRepairOrdersByBranchAsync(Guid branchId);
        #endregion

        #region Search and Filtering
        Task<IEnumerable<RoBoardCardDto>> SearchRepairOrdersAsync(string searchText, List<int> statusIds = null, List<Guid> branchIds = null, DateTime? fromDate = null, DateTime? toDate = null);
        #endregion

        #region Business Rules and Validation
        Task<bool> CanMoveToStatusAsync(Guid repairOrderId, int newStatusId);
        Task<IEnumerable<RoBoardLabelDto>> GetAvailableLabelsForStatusAsync(int statusId);
        #endregion

        #region Board Configuration
        Task<RoBoardConfigurationDto> GetBoardConfigurationAsync();
        Task<RoBoardPermissionsDto> GetUserPermissionsAsync(string userId, List<string> userRoles);
        Task<RepairOrder> UpdateRepairOrderStatusNoteServicesAsync(Guid repairOrderId, UpdateRepairOrderDto updateDto);
        #endregion

        #region Archive Management Operations
        Task<ArchiveOperationResultDto> ArchiveRepairOrderAsync(ArchiveRepairOrderDto archiveDto);
        Task<ArchiveOperationResultDto> RestoreRepairOrderAsync(RestoreRepairOrderDto restoreDto);
        Task<RoBoardListViewDto> GetArchivedRepairOrdersAsync(RoBoardFiltersDto filters = null, string sortBy = "ArchivedAt", string sortOrder = "Desc", int page = 1, int pageSize = 50);
        Task<ArchivedRepairOrderDetailDto> GetArchivedRepairOrderDetailAsync(Guid repairOrderId);
        Task<bool> IsRepairOrderArchivedAsync(Guid repairOrderId);
        #endregion

        #region Cancel Management Operations
        Task<ArchiveOperationResultDto> CancelRepairOrderAsync(CancelRepairOrderDto cancelDto);
        #endregion

        #region Label Management
        Task<RoBoardStatusUpdateResultDto> UpdateRepairOrderLabelsAsync(Guid repairOrderId, List<Guid> labelIds);
        #endregion

        #region Cost Calculation Methods
        /// <summary>
        /// Updates the RepairOrder cost based on completed inspection services
        /// This method is called when an inspection is completed but no quotation is created
        /// </summary>
        /// <param name="repairOrderId">The ID of the repair order to update</param>
        /// <returns>The updated repair order</returns>
        Task<RepairOrder> UpdateCostFromInspectionAsync(Guid repairOrderId);
        #endregion

        #region DTO Mapping Methods
        /// <summary>
        /// Maps a RepairOrder entity to a RepairOrderDto
        /// </summary>
        /// <param name="repairOrder">The RepairOrder entity to map</param>
        /// <returns>A RepairOrderDto</returns>
        RepairOrderDto MapToRepairOrderDto(RepairOrder repairOrder);
        #endregion

        #region Customer and Vehicle Info
        /// <summary>
        /// Gets customer and vehicle information for a repair order
        /// </summary>
        /// <param name="repairOrderId">The ID of the repair order</param>
        /// <returns>Customer and vehicle information DTO</returns>
        Task<Dtos.RepairOrder.RoCustomerVehicleInfoDto> GetCustomerVehicleInfoAsync(Guid repairOrderId);
        #endregion
    }
}