using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Branches;
using Dtos.RepairOrder;
using Dtos.RoBoard;
using Repositories;

namespace Services
{
    public class RepairOrderService : IRepairOrderService
    {
        private readonly IRepairOrderRepository _repairOrderRepository;
        private readonly IOrderStatusRepository _orderStatusRepository;
        private readonly ILabelRepository _labelRepository;

        // 3 status tuong ung voi 3 column 
        private readonly Dictionary<string, string> _statusNames = new Dictionary<string, string>
        {
            { "Pending", "Orders waiting to be processed" },
            { "In Progress", "Orders currently being worked on" },
            { "Completed", "Orders that have been finished" }
        };

        public RepairOrderService(
            IRepairOrderRepository repairOrderRepository,
            IOrderStatusRepository orderStatusRepository,
            ILabelRepository labelRepository)
        {
            _repairOrderRepository = repairOrderRepository;
            _orderStatusRepository = orderStatusRepository;
            _labelRepository = labelRepository;
        }

        #region Kanban Board Operations

        public async Task<RoBoardDto> GetKanbanBoardAsync(RoBoardFiltersDto filters = null)
        {
            var repairOrders = await _repairOrderRepository.GetRepairOrdersForKanbanAsync(filters);
            var allStatuses = await _orderStatusRepository.GetAllAsync();
            var statistics = await GetBoardStatisticsAsync(filters);

            var board = new RoBoardDto
            {
                BoardName = "Repair Orders Board",
                LastUpdated = DateTime.UtcNow,
                Statistics = statistics,
                Configuration = await GetBoardConfigurationAsync(),
                AppliedFilters = filters ?? new RoBoardFiltersDto()
            };

            // Create 3 columns based on simplified status logic
            foreach (var statusName in _statusNames.Keys)
            {
                var status = allStatuses.FirstOrDefault(s => s.StatusName == statusName);
                if (status != null)
                {
                    var statusOrders = repairOrders.Where(ro => ro.StatusId == status.OrderStatusId).ToList();
                    var column = new RoBoardColumnDto
                    {
                        OrderStatusId = status.OrderStatusId,
                        StatusName = status.StatusName,
                        OrderIndex = GetStatusOrderIndex(statusName),
                        Cards = statusOrders.Select(MapToRoBoardCardDto).ToList(),
                        AvailableLabels = (await _orderStatusRepository.GetLabelsByStatusIdAsync(status.OrderStatusId))
                            .Select(MapToRoBoardLabelDto).ToList()
                    };
                    board.Columns.Add(column);
                }
            }

            return board;
        }

        public async Task<RoBoardListViewDto> GetListViewAsync(
            RoBoardFiltersDto filters = null,
            string sortBy = "ReceiveDate",
            string sortOrder = "Desc",
            int page = 1,
            int pageSize = 50)
        {
            var (items, totalCount) = await _repairOrderRepository.GetRepairOrdersForListViewAsync(
                filters, sortBy, sortOrder, page, pageSize);

            var listView = new RoBoardListViewDto
            {
                Items = items.Select((ro, index) => MapToRoBoardListItemDto(ro, ((page - 1) * pageSize) + index + 1)).ToList(),
                Pagination = new RoBoardListPaginationDto
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount
                },
                Sorting = new RoBoardListSortingDto
                {
                    SortBy = sortBy,
                    SortOrder = sortOrder
                },
                AppliedFilters = filters ?? new RoBoardFiltersDto(),
                Statistics = await GetBoardStatisticsAsync(filters),
                LastUpdated = DateTime.UtcNow
            };

            return listView;
        }

        #endregion

        #region Drag & Drop Operations

        public async Task<RoBoardStatusUpdateResultDto> UpdateRepairOrderStatusAsync(UpdateRoBoardStatusDto updateDto)
        {
            var result = new RoBoardStatusUpdateResultDto
            {
                RepairOrderId = updateDto.RepairOrderId,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                // Validate the move first
                var validation = await ValidateMoveAsync(updateDto.RepairOrderId,
                    updateDto.PreviousStatusId ?? Guid.Empty, updateDto.NewStatusId);

                if (!validation.IsValid)
                {
                    result.Success = false;
                    result.Message = validation.ValidationMessage;
                    result.Errors.AddRange(validation.Requirements);
                    return result;
                }

                var repairOrder = await _repairOrderRepository.GetRepairOrderWithFullDetailsAsync(updateDto.RepairOrderId);
                if (repairOrder == null)
                {
                    result.Success = false;
                    result.Message = "Repair order not found";
                    result.Errors.Add("Invalid repair order ID");
                    return result;
                }

                result.OldStatusId = repairOrder.StatusId;
                result.NewStatusId = updateDto.NewStatusId;

                // Update the status
                var updateSuccess = await _repairOrderRepository.UpdateRepairOrderStatusAsync(
                    updateDto.RepairOrderId, updateDto.NewStatusId, updateDto.ChangeNote);

                if (updateSuccess)
                {
                    // Get updated repair order for response
                    var updatedRepairOrder = await _repairOrderRepository.GetRepairOrderWithFullDetailsAsync(updateDto.RepairOrderId);
                    result.UpdatedCard = MapToRoBoardCardDto(updatedRepairOrder);
                    result.Success = true;
                    result.Message = "Status updated successfully";
                }
                else
                {
                    result.Success = false;
                    result.Message = "Failed to update status";
                    result.Errors.Add("Database update failed");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "An error occurred while updating status";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<BatchRoBoardStatusUpdateResultDto> BatchUpdateRepairOrderStatusAsync(BatchUpdateRoBoardStatusDto batchUpdateDto)
        {
            var result = new BatchRoBoardStatusUpdateResultDto
            {
                ProcessedAt = DateTime.UtcNow,
                BatchMessage = batchUpdateDto.BatchNote ?? "Batch status update"
            };

            try
            {
                foreach (var update in batchUpdateDto.Updates)
                {
                    var singleResult = await UpdateRepairOrderStatusAsync(update);
                    result.Results.Add(singleResult);

                    if (singleResult.Success)
                        result.SuccessfulUpdates++;
                    else
                        result.FailedUpdates++;
                }

                result.OverallSuccess = result.FailedUpdates == 0;
            }
            catch (Exception ex)
            {
                result.OverallSuccess = false;
                result.BatchMessage = $"Batch operation failed: {ex.Message}";
            }

            return result;
        }

        public async Task<RoBoardMoveValidationDto> ValidateMoveAsync(Guid repairOrderId, Guid fromStatusId, Guid toStatusId)
        {
            var validation = new RoBoardMoveValidationDto
            {
                RepairOrderId = repairOrderId,
                FromStatusId = fromStatusId,
                ToStatusId = toStatusId
            };

            try
            {
                // Check if repair order exists
                var repairOrder = await _repairOrderRepository.GetRepairOrderWithFullDetailsAsync(repairOrderId);
                if (repairOrder == null)
                {
                    validation.IsValid = false;
                    validation.ValidationMessage = "Repair order not found";
                    return validation;
                }

                // Check if target status exists
                var targetStatus = await _orderStatusRepository.GetByIdAsync(toStatusId);
                if (targetStatus == null)
                {
                    validation.IsValid = false;
                    validation.ValidationMessage = "Target status not found";
                    return validation;
                }

                // Apply business rules based on simplified 3-status logic
                validation.IsValid = await ApplyBusinessRules(repairOrder, targetStatus);

                if (!validation.IsValid)
                {
                    validation.ValidationMessage = GetBusinessRuleMessage(repairOrder, targetStatus);
                    validation.Requirements = GetBusinessRuleRequirements(repairOrder, targetStatus);
                }
                else
                {
                    validation.ValidationMessage = "Move is valid";
                    // Get available labels for new status
                    validation.AvailableLabelsInNewStatus = (await GetAvailableLabelsForStatusAsync(toStatusId)).ToList();
                }
            }
            catch (Exception ex)
            {
                validation.IsValid = false;
                validation.ValidationMessage = $"Validation error: {ex.Message}";
            }

            return validation;
        }

        #endregion

        #region CRUD Operations

        public async Task<RoBoardCardDto?> GetRepairOrderCardAsync(Guid repairOrderId)
        {
            var repairOrder = await _repairOrderRepository.GetRepairOrderWithFullDetailsAsync(repairOrderId);
            return repairOrder != null ? MapToRoBoardCardDto(repairOrder) : null;
        }

        public async Task<RepairOrder> CreateRepairOrderAsync(RepairOrder repairOrder)
        {
            // Set default status to "Pending" if not specified
            if (repairOrder.StatusId == Guid.Empty)
            {
                var pendingStatus = (await _orderStatusRepository.GetAllAsync())
                    .FirstOrDefault(s => s.StatusName == "Pending");
                if (pendingStatus != null)
                {
                    repairOrder.StatusId = pendingStatus.OrderStatusId;
                }
            }

            return await _repairOrderRepository.CreateAsync(repairOrder);
        }

        public async Task<RepairOrder> UpdateRepairOrderAsync(RepairOrder repairOrder)
        {
            return await _repairOrderRepository.UpdateAsync(repairOrder);
        }

        public async Task<bool> DeleteRepairOrderAsync(Guid repairOrderId)
        {
            return await _repairOrderRepository.DeleteAsync(repairOrderId);
        }

        public async Task<IEnumerable<RepairOrder>> GetAllRepairOrdersAsync()
        {
            return await _repairOrderRepository.GetAllRepairOrdersWithFullDetailsAsync();
        }

        // NEW: Get repair orders by status
        public async Task<IEnumerable<RepairOrder>> GetRepairOrdersByStatusAsync(Guid statusId)
        {
            return await _repairOrderRepository.GetRepairOrdersByStatusAsync(statusId);
        }

        public async Task<RepairOrder> GetRepairOrderWithFullDetailsAsync(Guid repairOrderId)
        {
            return await _repairOrderRepository.GetRepairOrderWithFullDetailsAsync(repairOrderId);
        }

        public async Task<bool> RepairOrderExistsAsync(Guid repairOrderId)
        {
            return await _repairOrderRepository.ExistsAsync(repairOrderId);
        }

        #endregion

        #region Statistics and Analytics

        public async Task<RoBoardStatisticsDto> GetBoardStatisticsAsync(RoBoardFiltersDto filters = null)
        {
            var stats = await _repairOrderRepository.GetKanbanStatisticsAsync(filters);
            var statusCounts = await _repairOrderRepository.GetRepairOrderCountsByStatusAsync();
            var allStatuses = await _orderStatusRepository.GetAllAsync();

            var statistics = new RoBoardStatisticsDto
            {
                TotalRepairOrders = (int)(stats["TotalRepairOrders"] ?? 0),
                CompletedOrders = (int)(stats["CompletedOrders"] ?? 0),
                OverdueOrders = (int)(stats["OverdueOrders"] ?? 0),
                OrdersInProgress = (int)(stats["OrdersInProgress"] ?? 0),
                TotalRevenue = (decimal)(stats["TotalRevenue"] ?? 0m),
                PendingPayments = (decimal)(stats["PendingPayments"] ?? 0m)
            };

            // Map status counts
            foreach (var status in allStatuses)
            {
                if (statusCounts.ContainsKey(status.OrderStatusId))
                {
                    statistics.OrdersByStatus[status.StatusName] = statusCounts[status.OrderStatusId];
                }
            }

            return statistics;
        }

        public async Task<Dictionary<Guid, int>> GetRepairOrderCountsByStatusAsync(List<Guid> statusIds = null)
        {
            return await _repairOrderRepository.GetRepairOrderCountsByStatusAsync(statusIds);
        }

        #endregion

        #region User-specific Operations

        public async Task<IEnumerable<RoBoardCardDto>> GetRepairOrdersByUserAsync(string userId)
        {
            var repairOrders = await _repairOrderRepository.GetRepairOrdersByUserIdAsync(userId);
            return repairOrders.Select(MapToRoBoardCardDto);
        }

        public async Task<IEnumerable<RoBoardCardDto>> GetRepairOrdersByBranchAsync(Guid branchId)
        {
            var repairOrders = await _repairOrderRepository.GetRepairOrdersByBranchIdAsync(branchId);
            return repairOrders.Select(MapToRoBoardCardDto);
        }

        #endregion

        #region Search and Filtering

        public async Task<IEnumerable<RoBoardCardDto>> SearchRepairOrdersAsync(
            string searchText,
            List<Guid> statusIds = null,
            List<Guid> branchIds = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var repairOrders = await _repairOrderRepository.SearchRepairOrdersAsync(
                searchText, statusIds, branchIds, fromDate, toDate);
            return repairOrders.Select(MapToRoBoardCardDto);
        }

        #endregion

        #region Business Rules and Validation

        public async Task<bool> CanMoveToStatusAsync(Guid repairOrderId, Guid newStatusId)
        {
            return await _repairOrderRepository.CanMoveToStatusAsync(repairOrderId, newStatusId);
        }

        public async Task<IEnumerable<RoBoardLabelDto>> GetAvailableLabelsForStatusAsync(Guid statusId)
        {
            var labels = await _repairOrderRepository.GetAvailableLabelsForStatusAsync(statusId);
            return labels.Select(MapToRoBoardLabelDto);
        }

        #endregion

        #region Audit and History

        public async Task<IEnumerable<RoBoardCardDto>> GetRecentlyUpdatedRepairOrdersAsync(int hours = 24)
        {
            var repairOrders = await _repairOrderRepository.GetRecentlyUpdatedRepairOrdersAsync(hours);
            return repairOrders.Select(MapToRoBoardCardDto);
        }

        #endregion

        #region Simplified 3-Status Operations

        public async Task<RoBoardColumnDto> GetPendingOrdersAsync(RoBoardFiltersDto filters = null)
        {
            return await GetColumnByStatusNameAsync("Pending", filters);
        }

        public async Task<RoBoardColumnDto> GetInProgressOrdersAsync(RoBoardFiltersDto filters = null)
        {
            return await GetColumnByStatusNameAsync("In Progress", filters);
        }

        public async Task<RoBoardColumnDto> GetCompletedOrdersAsync(RoBoardFiltersDto filters = null)
        {
            return await GetColumnByStatusNameAsync("Completed", filters);
        }

        private async Task<RoBoardColumnDto> GetColumnByStatusNameAsync(string statusName, RoBoardFiltersDto filters)
        {
            var allStatuses = await _orderStatusRepository.GetAllAsync();
            var status = allStatuses.FirstOrDefault(s => s.StatusName == statusName);

            if (status == null)
            {
                return new RoBoardColumnDto
                {
                    StatusName = statusName,
                    OrderIndex = GetStatusOrderIndex(statusName)
                };
            }

            var repairOrders = await _repairOrderRepository.GetRepairOrdersByStatusAsync(status.OrderStatusId);

            return new RoBoardColumnDto
            {
                OrderStatusId = status.OrderStatusId,
                StatusName = status.StatusName,
                OrderIndex = GetStatusOrderIndex(statusName),
                Cards = repairOrders.Select(MapToRoBoardCardDto).ToList(),
                AvailableLabels = (await _orderStatusRepository.GetLabelsByStatusIdAsync(status.OrderStatusId))
                    .Select(MapToRoBoardLabelDto).ToList()
            };
        }

        #endregion

        #region Board Configuration

        public async Task<RoBoardConfigurationDto> GetBoardConfigurationAsync()
        {
            return new RoBoardConfigurationDto
            {
                AllowDragAndDrop = true,
                ShowCardDetails = true,
                ShowLabels = true,
                ShowCustomer = true,
                ShowVehicle = true,
                ShowDueDates = true,
                ShowProgress = true,
                DefaultSortBy = "ReceiveDate",
                DefaultSortOrder = "Desc",
                MaxCardsPerColumn = 100
            };
        }

        public async Task<RoBoardPermissionsDto> GetUserPermissionsAsync(string userId, List<string> userRoles)
        {
            // Basic permissions - extend based on user roles and business requirements
            var isManager = userRoles?.Contains("Manager") == true;

            return new RoBoardPermissionsDto
            {
                CanViewBoard = true,
                CanMoveCards = isManager,
                CanEditCards = isManager,
                CanDeleteCards = isManager,
                CanCreateCards = isManager,
                CanManageLabels = isManager,
                CanManageColumns = false, // Only system admins should manage columns
                CanViewStatistics = isManager
            };
        }

        #endregion

        #region DTO Mapping Methods

        private RoBoardCardDto MapToRoBoardCardDto(RepairOrder repairOrder)
        {
            return new RoBoardCardDto
            {
                RepairOrderId = repairOrder.RepairOrderId,
                ReceiveDate = repairOrder.ReceiveDate,
                EstimatedCompletionDate = repairOrder.EstimatedCompletionDate,
                CompletionDate = repairOrder.CompletionDate,
                Cost = repairOrder.Cost,
                EstimatedAmount = repairOrder.EstimatedAmount,
                PaidAmount = repairOrder.PaidAmount,
                PaidStatus = repairOrder.PaidStatus,
                EstimatedRepairTime = repairOrder.EstimatedRepairTime,
                Note = repairOrder.Note,
                CreatedAt = repairOrder.CreatedAt,
                StatusId = repairOrder.StatusId,
                StatusName = repairOrder.OrderStatus?.StatusName ?? "Unknown",
                Vehicle = MapToRoBoardVehicleDto(repairOrder.Vehicle),
                Customer = MapToRoBoardCustomerDto(repairOrder.User),
                Branch = MapToRoBoardBranchDto(repairOrder.Branch),
                AssignedLabels = repairOrder.OrderStatus?.Labels?.Select(MapToRoBoardLabelDto).ToList() ?? new List<RoBoardLabelDto>(),
                DaysInCurrentStatus = (int)(DateTime.UtcNow - repairOrder.CreatedAt).TotalDays,
                UpdatedAt = repairOrder.UpdatedAt,
                IsArchived = repairOrder.IsArchived,
                ArchivedAt = repairOrder.ArchivedAt,
                ArchivedBy = repairOrder.ArchivedByUserId
            };
        }

        // New method to map RepairOrder to enhanced RepairOrderDto
        public RepairOrderDto MapToRepairOrderDto(RepairOrder repairOrder)
        {
            var dto = new RepairOrderDto
            {
                RepairOrderId = repairOrder.RepairOrderId,
                RoType = repairOrder.RoType,
                ReceiveDate = repairOrder.ReceiveDate,
                EstimatedCompletionDate = repairOrder.EstimatedCompletionDate,
                CompletionDate = repairOrder.CompletionDate,
                Cost = repairOrder.Cost,
                EstimatedAmount = repairOrder.EstimatedAmount,
                PaidAmount = repairOrder.PaidAmount,
                PaidStatus = repairOrder.PaidStatus,
                EstimatedRepairTime = repairOrder.EstimatedRepairTime,
                Note = repairOrder.Note,
                CreatedAt = repairOrder.CreatedAt,
                UpdatedAt = repairOrder.UpdatedAt,
                IsArchived = repairOrder.IsArchived,
                ArchivedAt = repairOrder.ArchivedAt,
                ArchivedByUserId = repairOrder.ArchivedByUserId,
                BranchId = repairOrder.BranchId,
                StatusId = repairOrder.StatusId,
                VehicleId = repairOrder.VehicleId,
                UserId = repairOrder.UserId,
                RepairRequestId = repairOrder.RepairRequestId,
                CustomerName = repairOrder.User?.FullName ?? "Unknown Customer",
                CustomerPhone = repairOrder.User?.PhoneNumber ?? "",
            };

            // Add technician names (from jobs)
            if (repairOrder.Jobs != null)
            {
                var technicianNames = new HashSet<string>();
                foreach (var job in repairOrder.Jobs)
                {
                    if (job.JobTechnicians != null)
                    {
                        foreach (var jobTech in job.JobTechnicians)
                        {
                            if (jobTech.Technician?.User != null)
                            {
                                technicianNames.Add(jobTech.Technician.User.FullName ?? "Unknown Technician");
                            }
                        }
                    }
                }
                dto.TechnicianNames = technicianNames.ToList();
                
                // Calculate progress based on job completion
                dto.TotalJobs = repairOrder.Jobs.Count;
                dto.CompletedJobs = repairOrder.Jobs.Count(j => j.Status == BusinessObject.Enums.JobStatus.Completed);
                dto.ProgressPercentage = dto.TotalJobs > 0 ? (decimal)(dto.CompletedJobs * 100) / dto.TotalJobs : 0;
            }

            return dto;
        }

        private RoBoardListItemDto MapToRoBoardListItemDto(RepairOrder repairOrder, int rowNumber)
        {
            return new RoBoardListItemDto
            {
                RepairOrderId = repairOrder.RepairOrderId,
                RowNumber = rowNumber,
                ReceiveDate = repairOrder.ReceiveDate,
                EstimatedCompletionDate = repairOrder.EstimatedCompletionDate,
                CompletionDate = repairOrder.CompletionDate,
                EstimatedAmount = repairOrder.EstimatedAmount,
                PaidAmount = repairOrder.PaidAmount,
                PaidStatus = repairOrder.PaidStatus,
                StatusId = repairOrder.StatusId,
                StatusName = repairOrder.OrderStatus?.StatusName ?? "Unknown",
                StatusColor = repairOrder.OrderStatus?.Labels?.FirstOrDefault()?.Color?.HexCode ?? "#808080",
                Labels = repairOrder.OrderStatus?.Labels?.Select(MapToRoBoardLabelDto).ToList() ?? new List<RoBoardLabelDto>(),
                CustomerName = repairOrder.User?.FullName ?? "Unknown Customer",
                CustomerEmail = repairOrder.User?.Email ?? "",
                CustomerPhone = repairOrder.User?.PhoneNumber ?? "",
                VehicleLicensePlate = repairOrder.Vehicle?.LicensePlate ?? "",
                VehicleBrand = "Unknown Brand", // TODO: Add brand navigation when available
                VehicleModel = "Unknown Model", // TODO: Add model navigation when available
                VehicleColor = "Unknown Color", // TODO: Add color navigation when available
                BranchName = repairOrder.Branch?.BranchName ?? "Unknown Branch",
                //BranchAddress = repairOrder.Branch != null ? 
                //    $"{repairOrder.Branch.Street}, {repairOrder.Branch.Ward}, {repairOrder.Branch.District}, {repairOrder.Branch.City}" : "",
                DaysInCurrentStatus = (int)(DateTime.UtcNow - repairOrder.CreatedAt).TotalDays,
                StatusDuration = GetStatusDurationText((int)(DateTime.UtcNow - repairOrder.CreatedAt).TotalDays),
                Priority = GetPriorityLevel(repairOrder),
                CanEdit = true, // TODO: Apply user permissions
                CanDelete = true, // TODO: Apply user permissions
                CanChangeStatus = true, // TODO: Apply user permissions
                CanAddPayment = repairOrder.PaidAmount < repairOrder.EstimatedAmount,
                CreatedAt = repairOrder.CreatedAt,
                UpdatedAt = repairOrder.UpdatedAt,
                IsArchived = repairOrder.IsArchived,
                ArchivedAt = repairOrder.ArchivedAt,
            };
        }

        private RoBoardVehicleDto MapToRoBoardVehicleDto(Vehicle vehicle)
        {
            if (vehicle == null) return new RoBoardVehicleDto();

            return new RoBoardVehicleDto
            {
                VehicleId = vehicle.VehicleId,
                LicensePlate = vehicle.LicensePlate ?? "",
                VIN = vehicle.VIN ?? "",
                BrandName = "Unknown Brand", // TODO: Add brand navigation when available
                ModelName = "Unknown Model", // TODO: Add model navigation when available
                ColorName = "Unknown Color" // TODO: Add color navigation when available
            };
        }

        private RoBoardCustomerDto MapToRoBoardCustomerDto(BusinessObject.Authentication.ApplicationUser user)
        {
            if (user == null) return new RoBoardCustomerDto();

            return new RoBoardCustomerDto
            {
                UserId = user.Id,
                FullName = user.FullName ?? "Unknown Customer",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? ""
            };
        }

        private RoBoardBranchDto MapToRoBoardBranchDto(Branch branch)
        {
            if (branch == null) return new RoBoardBranchDto();

            return new RoBoardBranchDto
            {
                BranchId = branch.BranchId,
                BranchName = branch.BranchName ?? "Unknown Branch",
                //Address = branch.Address ?? "",
                PhoneNumber = branch.PhoneNumber ?? ""
            };
        }

        private RoBoardLabelDto MapToRoBoardLabelDto(Label label)
        {
            if (label == null) return new RoBoardLabelDto();

            return new RoBoardLabelDto
            {
                LabelId = label.LabelId,
                LabelName = label.LabelName,
                Description = label.Description,
                Color = new RoBoardColorDto
                {
                    ColorId = label.Color?.ColorId ?? Guid.Empty,
                    ColorName = label.Color?.ColorName ?? "Default",
                    HexCode = label.Color?.HexCode ?? "#808080"
                }
            };
        }

        #endregion

        #region Helper Methods

        private int GetStatusOrderIndex(string statusName)
        {
            return statusName switch
            {
                "Pending" => 1,
                "In Progress" => 2,
                "Completed" => 3,
                _ => 99
            };
        }

        private async Task<bool> ApplyBusinessRules(RepairOrder repairOrder, OrderStatus targetStatus)
        {
            // Business rule: Can't move to "Completed" if there are unpaid amounts
            if (targetStatus.StatusName == "Completed")
            {
                if (repairOrder.PaidAmount < repairOrder.EstimatedAmount)
                {
                    return false;
                }
            }

            // Business rule: Can't move from "Completed" back to other statuses
            if (repairOrder.OrderStatus?.StatusName == "Completed" && targetStatus.StatusName != "Completed")
            {
                return false;
            }

            return true;
        }

        private string GetBusinessRuleMessage(RepairOrder repairOrder, OrderStatus targetStatus)
        {
            if (targetStatus.StatusName == "Completed" && repairOrder.PaidAmount < repairOrder.EstimatedAmount)
            {
                return "Cannot complete order with outstanding payments";
            }

            if (repairOrder.OrderStatus?.StatusName == "Completed" && targetStatus.StatusName != "Completed")
            {
                return "Cannot move completed orders back to previous statuses";
            }

            return "Business rule validation failed";
        }

        private List<string> GetBusinessRuleRequirements(RepairOrder repairOrder, OrderStatus targetStatus)
        {
            var requirements = new List<string>();

            if (targetStatus.StatusName == "Completed" && repairOrder.PaidAmount < repairOrder.EstimatedAmount)
            {
                var outstanding = repairOrder.EstimatedAmount - repairOrder.PaidAmount;
                requirements.Add($"Complete payment of ${outstanding:F2} required");
            }

            return requirements;
        }

        private string GetStatusDurationText(int days)
        {
            return days switch
            {
                0 => "Today",
                1 => "1 day",
                < 7 => $"{days} days",
                < 30 => $"{days / 7} weeks",
                _ => $"{days / 30} months"
            };
        }

        private string GetPriorityLevel(RepairOrder repairOrder)
        {
            // Business logic for priority calculation
            if (repairOrder.EstimatedCompletionDate.HasValue &&
                repairOrder.EstimatedCompletionDate.Value < DateTime.UtcNow.AddDays(1))
            {
                return "High";
            }

            if (repairOrder.EstimatedAmount > 5000)
            {
                return "High";
            }

            if (repairOrder.EstimatedCompletionDate.HasValue &&
                repairOrder.EstimatedCompletionDate.Value < DateTime.UtcNow.AddDays(3))
            {
                return "Medium";
            }

            return "Low";
        }

        #endregion

        #region Archive Management Operations

        public async Task<ArchiveOperationResultDto> ArchiveRepairOrderAsync(ArchiveRepairOrderDto archiveDto)
        {
            var result = new ArchiveOperationResultDto
            {
                RepairOrderId = archiveDto.RepairOrderId
            };

            try
            {
                // Check if repair order exists and is not already archived
                var repairOrder = await _repairOrderRepository.GetByIdAsync(archiveDto.RepairOrderId);
                if (repairOrder == null)
                {
                    result.Success = false;
                    result.Message = "Repair order not found";
                    result.Errors.Add("Invalid repair order ID");
                    return result;
                }

                if (repairOrder.IsArchived)
                {
                    result.Success = false;
                    result.Message = "Repair order is already archived";
                    result.Warnings.Add("Order was previously archived");
                    return result;
                }

                // Archive the repair order
                var archiveSuccess = await _repairOrderRepository.ArchiveRepairOrderAsync(
                    archiveDto.RepairOrderId, archiveDto.ArchiveReason, archiveDto.ArchivedByUserId);

                if (archiveSuccess)
                {
                    result.Success = true;
                    result.IsArchived = true;
                    result.ArchivedAt = DateTime.UtcNow;
                    result.Message = "Repair order archived successfully";
                }
                else
                {
                    result.Success = false;
                    result.Message = "Failed to archive repair order";
                    result.Errors.Add("Database update failed");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "An error occurred while archiving repair order";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<ArchiveOperationResultDto> RestoreRepairOrderAsync(RestoreRepairOrderDto restoreDto)
        {
            var result = new ArchiveOperationResultDto
            {
                RepairOrderId = restoreDto.RepairOrderId
            };

            try
            {
                // Check if repair order exists and is archived
                var repairOrder = await _repairOrderRepository.GetByIdAsync(restoreDto.RepairOrderId);
                if (repairOrder == null)
                {
                    result.Success = false;
                    result.Message = "Repair order not found";
                    result.Errors.Add("Invalid repair order ID");
                    return result;
                }

                if (!repairOrder.IsArchived)
                {
                    result.Success = false;
                    result.Message = "Repair order is not archived";
                    result.Warnings.Add("Order is already active");
                    return result;
                }

                // Restore the repair order
                var restoreSuccess = await _repairOrderRepository.RestoreRepairOrderAsync(
                    restoreDto.RepairOrderId, restoreDto.RestoreReason, restoreDto.RestoredByUserId);

                if (restoreSuccess)
                {
                    result.Success = true;
                    result.IsArchived = false;
                    result.RestoredAt = DateTime.UtcNow;
                    result.Message = "Repair order restored successfully";
                }
                else
                {
                    result.Success = false;
                    result.Message = "Failed to restore repair order";
                    result.Errors.Add("Database update failed");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "An error occurred while restoring repair order";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<RoBoardListViewDto> GetArchivedRepairOrdersAsync(
            RoBoardFiltersDto filters = null,
            string sortBy = "ArchivedAt",
            string sortOrder = "Desc",
            int page = 1,
            int pageSize = 50)
        {
            var archivedOrders = await _repairOrderRepository.GetArchivedRepairOrdersAsync(filters);
            
            // Apply sorting
            var sortedOrders = sortBy?.ToLower() switch
            {
                "archivedat" => sortOrder?.ToLower() == "desc" 
                    ? archivedOrders.OrderByDescending(ro => ro.ArchivedAt)
                    : archivedOrders.OrderBy(ro => ro.ArchivedAt),
                "receivedate" => sortOrder?.ToLower() == "desc" 
                    ? archivedOrders.OrderByDescending(ro => ro.ReceiveDate)
                    : archivedOrders.OrderBy(ro => ro.ReceiveDate),
                _ => archivedOrders.OrderByDescending(ro => ro.ArchivedAt)
            };

            var totalCount = sortedOrders.Count();
            var pagedItems = sortedOrders.Skip((page - 1) * pageSize).Take(pageSize);

            var listView = new RoBoardListViewDto
            {
                Items = pagedItems.Select((ro, index) => MapToRoBoardListItemDto(ro, ((page - 1) * pageSize) + index + 1)).ToList(),
                Pagination = new RoBoardListPaginationDto
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount
                },
                Sorting = new RoBoardListSortingDto
                {
                    SortBy = sortBy,
                    SortOrder = sortOrder
                },
                AppliedFilters = filters ?? new RoBoardFiltersDto { OnlyArchived = true },
                LastUpdated = DateTime.UtcNow
            };

            return listView;
        }

        public async Task<bool> IsRepairOrderArchivedAsync(Guid repairOrderId)
        {
            return await _repairOrderRepository.IsRepairOrderArchivedAsync(repairOrderId);
        }

        #endregion
    }
}
