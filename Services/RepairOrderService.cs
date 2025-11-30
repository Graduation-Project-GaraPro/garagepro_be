using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;

using BusinessObject.Authentication;
using BusinessObject.Branches;
using Dtos.RepairOrder;
using Dtos.RoBoard;
using Microsoft.AspNetCore.SignalR;
using Services.Hubs; // Update namespace
using Repositories;
using Repositories.ServiceRepositories; // Add this for service repository
using Microsoft.EntityFrameworkCore;
using Services.FCMServices;
using BusinessObject.FcmDataModels;
using BusinessObject.Enums; // Add this for ToListAsync

namespace Services
{
    public class RepairOrderService : IRepairOrderService
    {
        private readonly IRepairOrderRepository _repairOrderRepository;
        private readonly IOrderStatusRepository _orderStatusRepository;
        private readonly IFcmService _fcmService;
        private readonly ILabelRepository _labelRepository;
        private readonly IHubContext<RepairOrderHub> _hubContext; // Update namespace
        private readonly IServiceRepository _serviceRepository; // Add service repository
        private readonly IUserService _userService; // Add service repository

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
            ILabelRepository labelRepository,
            IHubContext<RepairOrderHub> hubContext,
            IUserService userService,
            IServiceRepository serviceRepository, IFcmService fcmService) // Add service repository parameter
        {
            _repairOrderRepository = repairOrderRepository;
            _orderStatusRepository = orderStatusRepository;
            _labelRepository = labelRepository;
            _hubContext = hubContext;
            _serviceRepository = serviceRepository; // Initialize service repository
            _fcmService = fcmService;
            _userService = userService;
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
                // First, get the current repair order to get its current status
                var currentRepairOrder = await _repairOrderRepository.GetRepairOrderWithFullDetailsAsync(updateDto.RepairOrderId);
                if (currentRepairOrder == null)
                {
                    result.Success = false;
                    result.Message = "Repair order not found";
                    result.Errors.Add("Invalid repair order ID");
                    return result;
                }

                // Validate the move using the current status as the "from" status
                var validation = await ValidateMoveAsync(updateDto.RepairOrderId, currentRepairOrder.StatusId, updateDto.NewStatusId);

                if (!validation.IsValid)
                {
                    result.Success = false;
                    result.Message = validation.ValidationMessage;
                    result.Errors.AddRange(validation.Requirements);
                    return result;
                }

                result.OldStatusId = currentRepairOrder.StatusId;
                result.NewStatusId = updateDto.NewStatusId;

                // Update the status
                var updateSuccess = await _repairOrderRepository.UpdateRepairOrderStatusAsync(
                    updateDto.RepairOrderId, updateDto.NewStatusId, null);

                if (updateSuccess)
                {
                    // Auto-assign default label for the new status
                    var defaultLabel = await _labelRepository.GetDefaultLabelByStatusIdAsync(updateDto.NewStatusId);
                    if (defaultLabel != null)
                    {
                        await _repairOrderRepository.UpdateRepairOrderLabelsAsync(
                            updateDto.RepairOrderId, 
                            new List<Guid> { defaultLabel.LabelId });
                    }
                    else
                    {
                        // No default label, clear all labels
                        await _repairOrderRepository.UpdateRepairOrderLabelsAsync(updateDto.RepairOrderId, new List<Guid>());
                    }

                    // Get updated repair order for response
                    var updatedRepairOrder = await _repairOrderRepository.GetRepairOrderWithFullDetailsAsync(updateDto.RepairOrderId);
                    result.UpdatedCard = MapToRoBoardCardDto(updatedRepairOrder);
                    result.Success = true;
                    result.Message = "Status updated successfully";

                    // Send real-time update via SignalR
                    if (_hubContext != null)
                    {
                        await _hubContext.Clients.All.SendAsync("RepairOrderMoved", updateDto.RepairOrderId, updateDto.NewStatusId, result.UpdatedCard);
                    }
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
       

        public async Task<RoBoardMoveValidationDto> ValidateMoveAsync(Guid repairOrderId, int fromStatusId, int toStatusId) // Changed from Guid to int
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

        public async Task<RepairOrder> CreateRepairOrderAsync(RepairOrder repairOrder, List<Guid> selectedServiceIds = null)
        {
            // Set default status to "Pending" if not specified
            if (repairOrder.StatusId == 0) // Changed from Guid.Empty to 0 for int
            {
                var pendingStatus = (await _orderStatusRepository.GetAllAsync())
                    .FirstOrDefault(s => s.StatusName == "Pending");
                if (pendingStatus != null)
                {
                    repairOrder.StatusId = pendingStatus.OrderStatusId;
                }
                else
                {
                    // If no pending status exists, create a default one
                    var defaultStatus = new OrderStatus
                    {
                        StatusName = "Pending"
                    };
                    var createdStatus = await _orderStatusRepository.CreateAsync(defaultStatus);
                    repairOrder.StatusId = createdStatus.OrderStatusId;
                }
            }

            var createdRepairOrder = await _repairOrderRepository.CreateAsync(repairOrder);



            // Create RepairOrderService entries for selected services
            if (selectedServiceIds != null && selectedServiceIds.Any())
            {
                var services = await _serviceRepository.Query()
                    .Where(s => selectedServiceIds.Contains(s.ServiceId))
                    .ToListAsync();

                foreach (var service in services)
                {
                    var repairOrderService = new BusinessObject.RepairOrderService
                    {
                        RepairOrderId = createdRepairOrder.RepairOrderId,
                        ServiceId = service.ServiceId,
                        ActualDuration = service.EstimatedDuration,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Add to context but don't save yet
                    _repairOrderRepository.Context.RepairOrderServices.Add(repairOrderService);
                }

                // Save all RepairOrderService entries
                await _repairOrderRepository.Context.SaveChangesAsync();
            }

            // Send real-time update via SignalR
            if (_hubContext != null)
            {
                var cardDto = MapToRoBoardCardDto(createdRepairOrder);
                await _hubContext.Clients.All.SendAsync("RepairOrderCreated", cardDto);
            }

            return createdRepairOrder;
        }

        public async Task<RepairOrder> UpdateRepairOrderAsync(RepairOrder repairOrder)
        {
            var updatedRepairOrder = await _repairOrderRepository.UpdateAsync(repairOrder);

            // Send real-time update via SignalR
            if (_hubContext != null && updatedRepairOrder != null)
            {
                var cardDto = MapToRoBoardCardDto(updatedRepairOrder);
                await _hubContext.Clients.All.SendAsync("RepairOrderUpdated", cardDto);
            }

            return updatedRepairOrder;
        }

        public async Task<bool> DeleteRepairOrderAsync(Guid repairOrderId)
        {
            var result = await _repairOrderRepository.DeleteAsync(repairOrderId);

            // Send real-time update via SignalR
            if (_hubContext != null && result)
            {
                await _hubContext.Clients.All.SendAsync("RepairOrderDeleted", repairOrderId);
            }

            return result;
        }

        public async Task<IEnumerable<RepairOrder>> GetAllRepairOrdersAsync()
        {
            return await _repairOrderRepository.GetAllRepairOrdersWithFullDetailsAsync();
        }

        // NEW: Get repair orders by status
        public async Task<IEnumerable<RepairOrder>> GetRepairOrdersByStatusAsync(int statusId) // Changed from Guid to int
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

        public async Task<Dictionary<int, int>> GetRepairOrderCountsByStatusAsync(List<int> statusIds = null) // Changed from Guid to int
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
            List<int> statusIds = null, // Changed from Guid to int
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

        public async Task<bool> CanMoveToStatusAsync(Guid repairOrderId, int newStatusId) // Changed from Guid to int
        {
            return await _repairOrderRepository.CanMoveToStatusAsync(repairOrderId, newStatusId);
        }

        public async Task<IEnumerable<RoBoardLabelDto>> GetAvailableLabelsForStatusAsync(int statusId) // Changed from Guid to int
        {
            var labels = await _repairOrderRepository.GetAvailableLabelsForStatusAsync(statusId);
            return labels.Select(MapToRoBoardLabelDto);
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

        public async Task<RepairOrder> UpdateRepairOrderStatusNoteServicesAsync(Guid repairOrderId, UpdateRepairOrderDto updateDto)
        {
            // First, get the existing repair order with its services
            var existingRepairOrder = await _repairOrderRepository.GetRepairOrderWithFullDetailsAsync(repairOrderId);
            if (existingRepairOrder == null)
            {
                throw new ArgumentException($"Repair order with ID {repairOrderId} not found.");
            }

            // Update status and note
            existingRepairOrder.StatusId = updateDto.StatusId;
            existingRepairOrder.Note = updateDto.Note;
            existingRepairOrder.UpdatedAt = DateTime.UtcNow;

            // Update services if provided
            if (updateDto.SelectedServiceIds != null)
            {
                // Remove existing services
                if (existingRepairOrder.RepairOrderServices != null)
                {
                    _repairOrderRepository.Context.RepairOrderServices.RemoveRange(existingRepairOrder.RepairOrderServices);
                }

                // Add new services
                var services = await _serviceRepository.Query()
                    .Where(s => updateDto.SelectedServiceIds.Contains(s.ServiceId))
                    .ToListAsync();

                var repairOrderServices = new List<BusinessObject.RepairOrderService>();
                decimal totalEstimatedAmount = 0;
                long totalEstimatedTime = 0;

                foreach (var service in services)
                {
                    var repairOrderService = new BusinessObject.RepairOrderService
                    {
                        RepairOrderId = existingRepairOrder.RepairOrderId,
                        ServiceId = service.ServiceId,
                        ActualDuration = service.EstimatedDuration,
                        CreatedAt = DateTime.UtcNow
                    };
                    repairOrderServices.Add(repairOrderService);

                    totalEstimatedAmount += service.Price;
                    totalEstimatedTime += (long)(service.EstimatedDuration * 60); // Convert hours to minutes
                }

                // Update the calculated fields
                existingRepairOrder.EstimatedAmount = totalEstimatedAmount;
                existingRepairOrder.EstimatedRepairTime = totalEstimatedTime;

                // Add new services to context
                _repairOrderRepository.Context.RepairOrderServices.AddRange(repairOrderServices);
            }

            // Save changes
            await _repairOrderRepository.Context.SaveChangesAsync();

            // Return updated repair order
            return await _repairOrderRepository.GetRepairOrderWithFullDetailsAsync(repairOrderId);
        }

        #endregion

        #region Cost Calculation Methods

        /// <summary>
        /// Updates the RepairOrder cost based on completed inspection services
        /// This method is called when an inspection is completed but no quotation is created
        /// </summary>
        /// <param name="repairOrderId">The ID of the repair order to update</param>
        /// <returns>The updated repair order</returns>
        public async Task<RepairOrder> UpdateCostFromInspectionAsync(Guid repairOrderId)
        {
            // Get the repair order with inspections and service inspections
            var repairOrder = await _repairOrderRepository.GetRepairOrderWithFullDetailsAsync(repairOrderId);
            if (repairOrder == null)
                throw new ArgumentException("Repair order not found");

            // Check if there are any completed inspections without approved quotations
            var completedInspections = repairOrder.Inspections?.Where(i => i.Status == BusinessObject.Enums.InspectionStatus.Completed).ToList();
            if (completedInspections == null || !completedInspections.Any())
                return repairOrder;

            // Check if any of these inspections have approved quotations
            var inspectionsWithApprovedQuotes = completedInspections
                .Where(i => i.Quotations?.Any(q => q.Status == BusinessObject.Enums.QuotationStatus.Approved) == true)
                .ToList();

            // If all completed inspections have approved quotations, don't update the cost
            // The cost should already be updated through the quotation approval process
            if (inspectionsWithApprovedQuotes.Count == completedInspections.Count)
                return repairOrder;

            // Calculate cost from inspection services for inspections without approved quotations
            decimal totalCost = 0;

            foreach (var inspection in completedInspections)
            {
                // Skip inspections that have approved quotations
                if (inspection.Quotations?.Any(q => q.Status == BusinessObject.Enums.QuotationStatus.Approved) == true)
                    continue;

                // Add cost of services in this inspection
                if (inspection.ServiceInspections != null)
                {
                    foreach (var serviceInspection in inspection.ServiceInspections)
                    {
                        if (serviceInspection.Service != null)
                        {
                            totalCost += serviceInspection.Service.Price;
                        }
                    }
                }
            }

            // Update the repair order cost only if it's different
            if (repairOrder.Cost != totalCost)
            {
                repairOrder.Cost = totalCost;
                repairOrder.UpdatedAt = DateTime.UtcNow;

                // Save changes
                var updatedRepairOrder = await _repairOrderRepository.UpdateAsync(repairOrder);
                return updatedRepairOrder;
            }

            return repairOrder;
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
                AssignedLabels = repairOrder.Labels?.Select(MapToRoBoardLabelDto).ToList() ?? new List<RoBoardLabelDto>(),
                DaysInCurrentStatus = (int)(DateTime.UtcNow - repairOrder.CreatedAt).TotalDays,
                UpdatedAt = repairOrder.UpdatedAt,
                // Archive Management
                IsArchived = repairOrder.IsArchived,
                ArchivedAt = repairOrder.ArchivedAt,
                ArchivedBy = repairOrder.ArchivedByUserId,
                // Cancellation Management
                IsCancelled = repairOrder.IsCancelled,
                CancelledAt = repairOrder.CancelledAt,
                CancelReason = repairOrder.CancelReason
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
                CustomerName = repairOrder.User != null ? $"{repairOrder.User.FirstName} {repairOrder.User.LastName}".Trim() : "Unknown Customer",
                CustomerEmail = repairOrder.User?.Email ?? "",
                CustomerPhone = repairOrder.User?.PhoneNumber ?? "",
                IsCancelled = repairOrder.IsCancelled,
                CancelledAt = repairOrder.CancelledAt,
                CancelReason = repairOrder.CancelReason
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
                            technicianNames.Add(jobTech.Technician.User != null ? $"{jobTech.Technician.User.FirstName} {jobTech.Technician.User.LastName}".Trim() : "Unknown Technician");
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
                StatusColor = repairOrder.OrderStatus?.Labels?.FirstOrDefault()?.HexCode ?? "#808080",
                Labels = repairOrder.Labels?.Select(MapToRoBoardLabelDto).ToList() ?? new List<RoBoardLabelDto>(),
                CustomerName = repairOrder.User != null ? $"{repairOrder.User.FirstName} {repairOrder.User.LastName}".Trim() : "Unknown Customer",
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
                BrandName = vehicle.Brand?.BrandName ?? "Unknown Brand",
                ModelName = vehicle.Model?.ModelName ?? "Unknown Model",
                ColorName = vehicle.Color?.ColorName ?? "Unknown Color"
            };
        }

        private RoBoardCustomerDto MapToRoBoardCustomerDto(BusinessObject.Authentication.ApplicationUser user)
        {
            if (user == null) return new RoBoardCustomerDto();

            return new RoBoardCustomerDto
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
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
                ColorName = label.ColorName ?? "Default",
                HexCode = label.HexCode ?? "#808080",
                OrderStatusId = label.OrderStatusId,
                Color = new RoBoardColorDto
                {
                    ColorName = label.ColorName ?? "Default",
                    HexCode = label.HexCode ?? "#808080"
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
            // Business rule: Can't move from "Completed" back to other statuses
            if (repairOrder.OrderStatus?.StatusName == "Completed" && targetStatus.StatusName != "Completed")
            {
                return false;
            }

            // Business rule: Can't complete RO if it has incomplete jobs
            if (targetStatus.StatusName == "Completed")
            {
                if (repairOrder.Jobs != null && repairOrder.Jobs.Any())
                {
                    var incompleteJobs = repairOrder.Jobs
                        .Where(j => j.Status != BusinessObject.Enums.JobStatus.Completed)
                        .ToList();

                    if (incompleteJobs.Any())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private string GetBusinessRuleMessage(RepairOrder repairOrder, OrderStatus targetStatus)
        {
            if (repairOrder.OrderStatus?.StatusName == "Completed" && targetStatus.StatusName != "Completed")
            {
                return "Cannot move completed orders back to previous statuses";
            }

            if (targetStatus.StatusName == "Completed")
            {
                if (repairOrder.Jobs != null && repairOrder.Jobs.Any())
                {
                    var incompleteJobs = repairOrder.Jobs
                        .Where(j => j.Status != BusinessObject.Enums.JobStatus.Completed)
                        .ToList();

                    if (incompleteJobs.Any())
                    {
                        return $"Cannot complete repair order: {incompleteJobs.Count} job(s) are not completed";
                    }
                }
            }

            return "Business rule validation failed";
        }

        private List<string> GetBusinessRuleRequirements(RepairOrder repairOrder, OrderStatus targetStatus)
        {
            var requirements = new List<string>();

            // REMOVED: Payment requirement - now allowing completion with outstanding payments
            // if (targetStatus.StatusName == "Completed" && repairOrder.PaidAmount < repairOrder.EstimatedAmount)
            // {
            //     var outstanding = repairOrder.EstimatedAmount - repairOrder.PaidAmount;
            //     requirements.Add($"Complete payment of ${outstanding:F2} required");
            // }

            // Requirement: All jobs must be completed before completing RO
            if (targetStatus.StatusName == "Completed")
            {
                if (repairOrder.Jobs != null && repairOrder.Jobs.Any())
                {
                    var incompleteJobs = repairOrder.Jobs
                        .Where(j => j.Status != BusinessObject.Enums.JobStatus.Completed)
                        .ToList();

                    if (incompleteJobs.Any())
                    {
                        foreach (var job in incompleteJobs)
                        {
                            requirements.Add($"Job '{job.JobName}' must be completed (Current status: {job.Status})");
                        }
                    }
                }
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

                    await _hubContext.Clients.All.SendAsync(
                                "RepairOrderArchived",
                                archiveDto.RepairOrderId
                            );

                    var user = await _userService.GetUserByIdAsync(repairOrder.UserId);

                    if (user != null && user.DeviceId != null)
                    {
                        var FcmNotification = new FcmDataPayload
                        {
                            Type = NotificationType.Repair,
                            Title = "Repair Order Completed",
                            Body = "Your repair order has been completed. Thank you for using our service.",
                            EntityKey = EntityKeyType.repairOrderId,
                            EntityId = repairOrder.RepairOrderId,
                            Screen = AppScreen.RepairOrderArchivedDetailFragment
                        };
                        await _fcmService.SendFcmMessageAsync(user.DeviceId, FcmNotification);
                    }
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

        #region Cancel Management Operations

        public async Task<ArchiveOperationResultDto> CancelRepairOrderAsync(CancelRepairOrderDto cancelDto)
        {
            var result = new ArchiveOperationResultDto
            {
                RepairOrderId = cancelDto.RepairOrderId
            };

            try
            {
                // Check if repair order exists and is not already cancelled
                var repairOrder = await _repairOrderRepository.GetByIdAsync(cancelDto.RepairOrderId);
                if (repairOrder == null)
                {
                    result.Success = false;
                    result.Message = "Repair order not found";
                    result.Errors.Add("Invalid repair order ID");
                    return result;
                }

                if (repairOrder.IsCancelled)
                {
                    result.Success = false;
                    result.Message = "Repair order is already cancelled";
                    result.Warnings.Add("Order was previously cancelled");
                    return result;
                }

                // Cancel the repair order
                repairOrder.IsCancelled = true;
                repairOrder.CancelledAt = DateTime.UtcNow;
                repairOrder.CancelReason = cancelDto.CancelReason;
                repairOrder.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _repairOrderRepository.UpdateAsync(repairOrder);

                if (updateResult != null)
                {
                    result.Success = true;
                    result.IsArchived = true; // Using IsArchived property to indicate cancellation
                    result.ArchivedAt = DateTime.UtcNow;
                    result.Message = "Repair order cancelled successfully";
                }
                else
                {
                    result.Success = false;
                    result.Message = "Failed to cancel repair order";
                    result.Errors.Add("Database update failed");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "An error occurred while cancelling repair order";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        #endregion

        #region Label Management

        public async Task<RoBoardStatusUpdateResultDto> UpdateRepairOrderLabelsAsync(Guid repairOrderId, List<Guid> labelIds)
        {
            var result = new RoBoardStatusUpdateResultDto
            {
                RepairOrderId = repairOrderId,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                // Get the repair order
                var repairOrder = await _repairOrderRepository.GetRepairOrderWithFullDetailsAsync(repairOrderId);
                if (repairOrder == null)
                {
                    result.Success = false;
                    result.Message = "Repair order not found";
                    result.Errors.Add("Invalid repair order ID");
                    return result;
                }

                // Validate that all labels belong to the current status
                if (labelIds != null && labelIds.Any())
                {
                    var labels = await _labelRepository.GetByIdsAsync(labelIds);
                    var invalidLabels = labels.Where(l => l.OrderStatusId != repairOrder.StatusId).ToList();

                    if (invalidLabels.Any())
                    {
                        result.Success = false;
                        result.Message = "Some labels do not belong to the current status";
                        result.Errors.Add($"Invalid labels: {string.Join(", ", invalidLabels.Select(l => l.LabelName))}");
                        return result;
                    }

                    // Update labels
                    await _repairOrderRepository.UpdateRepairOrderLabelsAsync(repairOrderId, labelIds);
                }
                else
                {
                    // Clear all labels
                    await _repairOrderRepository.UpdateRepairOrderLabelsAsync(repairOrderId, new List<Guid>());
                }

                // Get updated repair order
                var updatedRepairOrder = await _repairOrderRepository.GetRepairOrderWithFullDetailsAsync(repairOrderId);
                result.UpdatedCard = MapToRoBoardCardDto(updatedRepairOrder);
                result.Success = true;
                result.Message = "Labels updated successfully";

                // Send real-time update via SignalR
                if (_hubContext != null)
                {
                    await _hubContext.Clients.All.SendAsync("RepairOrderLabelsUpdated", repairOrderId, result.UpdatedCard);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "An error occurred while updating labels";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        #endregion
    }
}