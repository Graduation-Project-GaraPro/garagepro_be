using System;
using System.Collections.Generic;
using BusinessObject.Enums;

namespace Dtos.RoBoard
{
    public class RoBoardListResponseDto
    {
        public List<RoBoardListItemDto> Items { get; set; } = new List<RoBoardListItemDto>();
        
        public RoBoardListPaginationDto Pagination { get; set; } = new RoBoardListPaginationDto();
        
        public RoBoardListSortingDto Sorting { get; set; } = new RoBoardListSortingDto();
        
        public RoBoardListColumnDto[] Columns { get; set; } = Array.Empty<RoBoardListColumnDto>();
        
        public RoBoardPermissionsDto Permissions { get; set; } = new RoBoardPermissionsDto();
    }
    
    public class RoBoardListViewDto
    {
        public List<RoBoardListItemDto> Items { get; set; } = new List<RoBoardListItemDto>();
        
        public RoBoardListPaginationDto Pagination { get; set; } = new RoBoardListPaginationDto();
        
        public RoBoardListSortingDto Sorting { get; set; } = new RoBoardListSortingDto();
        
        public RoBoardListColumnDto[] Columns { get; set; } = Array.Empty<RoBoardListColumnDto>();
        
        public RoBoardPermissionsDto Permissions { get; set; } = new RoBoardPermissionsDto();
        
        public RoBoardFiltersDto AppliedFilters { get; set; } = new RoBoardFiltersDto();
        
        public RoBoardStatisticsDto Statistics { get; set; } = new RoBoardStatisticsDto();
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
    
    public class RoBoardListItemDto
    {
        public Guid RepairOrderId { get; set; }
        
        public int RowNumber { get; set; }
        
        // Basic Info
        public string RepairOrderType { get; set; }
        
        public DateTime ReceiveDate { get; set; }
        
        public DateTime? EstimatedCompletionDate { get; set; }
        
        public DateTime? CompletionDate { get; set; }
        
        // Financial Info
        public decimal EstimatedAmount { get; set; }
        
        public decimal PaidAmount { get; set; }
        
        public PaidStatus PaidStatus { get; set; }
        
        public decimal CompletionPercentage => EstimatedAmount > 0 ? (PaidAmount / EstimatedAmount) * 100 : 0;
        
        // Status Info
        public int StatusId { get; set; }
        
        public string StatusName { get; set; }
        
        public string StatusColor { get; set; }
        
        public List<RoBoardLabelDto> Labels { get; set; } = new List<RoBoardLabelDto>();
        
        // Customer Info (flattened for table display)
        public string CustomerName { get; set; }
        
        public string CustomerEmail { get; set; }
        
        public string CustomerPhone { get; set; }
        
        // Vehicle Info (flattened for table display)
        public string VehicleLicensePlate { get; set; }
        
        public string VehicleBrand { get; set; }
        
        public string VehicleModel { get; set; }
        
        public string VehicleColor { get; set; }
        
        // Branch Info
        public string BranchName { get; set; }
        
        public string BranchAddress { get; set; }
        
        // Computed Properties for List View
        public bool IsOverdue => EstimatedCompletionDate.HasValue && 
                                EstimatedCompletionDate.Value < DateTime.UtcNow && 
                                CompletionDate == null;
        
        public int DaysInCurrentStatus { get; set; }
        
        public string StatusDuration { get; set; } // "3 days", "2 weeks", etc.
        
        public string Priority { get; set; } // High, Medium, Low based on business rules
        
        public decimal OutstandingAmount => EstimatedAmount - PaidAmount;
        
        // Quick Action Flags
        public bool CanEdit { get; set; }
        
        public bool CanDelete { get; set; }
        
        public bool CanChangeStatus { get; set; }
        
        public bool CanAddPayment { get; set; }
        
        // Display Helpers
        public string DisplayTitle => $"{RepairOrderType} - {VehicleLicensePlate}";
        
        public string DisplaySubtitle => $"{CustomerName} | {StatusName}";
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        // Archive Management
        public bool IsArchived { get; set; }
        
        public DateTime? ArchivedAt { get; set; }
        
        public string ArchiveReason { get; set; }
    }
    
    public class RoBoardListPaginationDto
    {
        public int CurrentPage { get; set; } = 1;
        
        public int PageSize { get; set; } = 50;
        
        public int TotalItems { get; set; }
        
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        
        public bool HasPreviousPage => CurrentPage > 1;
        
        public bool HasNextPage => CurrentPage < TotalPages;
        
        public int StartItem => ((CurrentPage - 1) * PageSize) + 1;
        
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
    }
    
    public class RoBoardListSortingDto
    {
        public string SortBy { get; set; } = "ReceiveDate";
        
        public string SortOrder { get; set; } = "Desc"; // Asc, Desc
        
        public List<RoBoardSortOptionDto> AvailableSortOptions { get; set; } = new List<RoBoardSortOptionDto>
        {
            new RoBoardSortOptionDto { Field = "ReceiveDate", DisplayName = "Receive Date" },
            new RoBoardSortOptionDto { Field = "EstimatedCompletionDate", DisplayName = "Due Date" },
            new RoBoardSortOptionDto { Field = "CompletionDate", DisplayName = "Completion Date" },
            new RoBoardSortOptionDto { Field = "EstimatedAmount", DisplayName = "Amount" },
            new RoBoardSortOptionDto { Field = "CustomerName", DisplayName = "Customer" },
            new RoBoardSortOptionDto { Field = "StatusName", DisplayName = "Status" },
            new RoBoardSortOptionDto { Field = "RepairOrderType", DisplayName = "Type" },
            new RoBoardSortOptionDto { Field = "VehicleLicensePlate", DisplayName = "Vehicle" },
            new RoBoardSortOptionDto { Field = "CreatedAt", DisplayName = "Created Date" }
        };
    }
    
    public class RoBoardSortOptionDto
    {
        public string Field { get; set; }
        
        public string DisplayName { get; set; }
        
        public bool IsNumeric { get; set; }
        
        public bool IsDate { get; set; }
    }
    
    public class RoBoardListColumnDto
    {
        public string Field { get; set; }
        
        public string DisplayName { get; set; }
        
        public bool IsVisible { get; set; } = true;
        
        public bool IsSortable { get; set; } = true;
        
        public bool IsFilterable { get; set; } = true;
        
        public int Width { get; set; } = 150; // pixels
        
        public string DataType { get; set; } = "text"; // text, number, date, currency, boolean, enum
        
        public string Format { get; set; } // For dates, numbers, currency formatting
        
        public int OrderIndex { get; set; }
        
        public bool IsFixed { get; set; } = false; // Fixed column (doesn't scroll)
        
        public string CssClass { get; set; }
    }
}