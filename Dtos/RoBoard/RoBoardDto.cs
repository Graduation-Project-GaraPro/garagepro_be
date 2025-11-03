using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BusinessObject.Enums;

namespace Dtos.RoBoard
{
    public class RoBoardDto
    {
        public Guid BoardId { get; set; } = Guid.NewGuid();
        
        [Required]
        public string BoardName { get; set; } = "Repair Orders Board";
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        public List<RoBoardColumnDto> Columns { get; set; } = new List<RoBoardColumnDto>();
        
        // Board-level statistics
        public RoBoardStatisticsDto Statistics { get; set; } = new RoBoardStatisticsDto();
        
        // Board configuration
        public RoBoardConfigurationDto Configuration { get; set; } = new RoBoardConfigurationDto();
        
        // User permissions for this board
        public RoBoardPermissionsDto Permissions { get; set; } = new RoBoardPermissionsDto();
        
        // Filters applied to the board
        public RoBoardFiltersDto AppliedFilters { get; set; } = new RoBoardFiltersDto();
        
        public int TotalCards => Columns?.Sum(c => c.RepairOrderCount) ?? 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string CreatedBy { get; set; }
    }
    
    public class RoBoardStatisticsDto
    {
        public int TotalRepairOrders { get; set; }
        
        public int CompletedOrders { get; set; }
        
        public int OverdueOrders { get; set; }
        
        public int OrdersInProgress { get; set; }
        
        public decimal TotalRevenue { get; set; }
        
        public decimal PendingPayments { get; set; }
        
        public double AverageCompletionTime { get; set; } // in days
        
        public Dictionary<string, int> OrdersByStatus { get; set; } = new Dictionary<string, int>();
        
        public Dictionary<string, decimal> RevenueByStatus { get; set; } = new Dictionary<string, decimal>();
    }
    
    public class RoBoardConfigurationDto
    {
        public bool AllowDragAndDrop { get; set; } = true;
        
        public bool ShowCardDetails { get; set; } = true;
        
        public bool ShowLabels { get; set; } = true;
        
        public bool ShowCustomer { get; set; } = true;
        
        public bool ShowVehicle { get; set; } = true;
        
        public bool ShowDueDates { get; set; } = true;
        
        public bool ShowProgress { get; set; } = true;
        
        public bool AutoRefresh { get; set; } = false;
        
        public int AutoRefreshInterval { get; set; } = 30; // seconds
        
        public string DefaultSortBy { get; set; } = "CreatedAt";
        
        public string DefaultSortOrder { get; set; } = "Desc";
        
        public int MaxCardsPerColumn { get; set; } = 100;
    }
    
    public class RoBoardPermissionsDto
    {
        public bool CanViewBoard { get; set; } = true;
        
        public bool CanMoveCards { get; set; } = false;
        
        public bool CanEditCards { get; set; } = false;
        
        public bool CanDeleteCards { get; set; } = false;
        
        public bool CanCreateCards { get; set; } = false;
        
        public bool CanManageLabels { get; set; } = false;
        
        public bool CanManageColumns { get; set; } = false;
        
        public bool CanViewStatistics { get; set; } = true;
        
        public List<Guid> RestrictedStatusIds { get; set; } = new List<Guid>();
        
        public List<Guid> AccessibleBranchIds { get; set; } = new List<Guid>();
    }
    
    public class RoBoardFiltersDto
    {
        public List<Guid> BranchIds { get; set; } = new List<Guid>();
        
        public List<int> StatusIds { get; set; } = new List<int>(); // Changed from Guid to int
        
        public List<Guid> LabelIds { get; set; } = new List<Guid>();
        
        public List<string> CustomerIds { get; set; } = new List<string>();
        
        public DateTime? FromDate { get; set; }
        
        public DateTime? ToDate { get; set; }
        
        public decimal? MinAmount { get; set; }
        
        public decimal? MaxAmount { get; set; }
        
        public string SearchText { get; set; }
        
        public bool? IsOverdue { get; set; }
        
        public List<PaidStatus> PaidStatuses { get; set; } = new List<PaidStatus>();
        
        public string RepairOrderType { get; set; }
        
        // Archive filtering
        public bool? IncludeArchived { get; set; } = false;
        
        public bool? OnlyArchived { get; set; } = false;
        
        public DateTime? ArchivedFromDate { get; set; }
        
        public DateTime? ArchivedToDate { get; set; }
    }
    
    public class RoBoardViewOptionsDto
    {
        public bool CompactView { get; set; } = false;
        
        public bool GroupByBranch { get; set; } = false;
        
        public bool GroupByCustomer { get; set; } = false;
        
        public bool ShowArchivedCards { get; set; } = false;
        
        public int CardsPerPage { get; set; } = 50;
        
        public int CurrentPage { get; set; } = 1;
        
        public string ViewMode { get; set; } = "kanban"; // kanban, list, card, table
        
        // List/Table view specific options
        public List<string> VisibleColumns { get; set; } = new List<string>
        {
            "RepairOrderType", "ReceiveDate", "Customer", "Vehicle", 
            "Status", "EstimatedAmount", "PaidAmount", "CompletionDate"
        };
        
        public string SortBy { get; set; } = "ReceiveDate";
        
        public string SortOrder { get; set; } = "Desc"; // Asc, Desc
        
        public bool ShowRowNumbers { get; set; } = true;
        
        public bool ShowQuickActions { get; set; } = true;
        
        public bool EnableInlineEditing { get; set; } = false;
        
        public bool ShowNestedLabels { get; set; } = true;
        
        public bool GroupByStatus { get; set; } = false;
        
        public bool ShowStatusColors { get; set; } = true;
        
        // Export options for list view
        public bool EnableExport { get; set; } = true;
        
        public List<string> ExportFormats { get; set; } = new List<string> { "csv", "excel", "pdf" };
    }
}