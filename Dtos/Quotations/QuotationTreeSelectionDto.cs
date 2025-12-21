using System;
using System.Collections.Generic;

namespace Dtos.Quotations
{
    public class ServiceCategoryTreeNodeDto
    {
        public Guid ServiceCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public Guid? ParentServiceCategoryId { get; set; }
        public bool HasChildren { get; set; }
        public int ServiceCount { get; set; }
        public int ChildCategoryCount { get; set; }
    }

    public class ServiceForSelectionDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public double EstimatedDuration { get; set; }
        public bool IsAdvanced { get; set; }
        public Guid ServiceCategoryId { get; set; }
        public string ServiceCategoryName { get; set; }
    }

    public class ServiceCategoryTreeResponseDto
    {
        public Guid? CurrentCategoryId { get; set; }
        public string CurrentCategoryName { get; set; }
        public List<ServiceCategoryTreeNodeDto> ChildCategories { get; set; }
        public List<ServiceForSelectionDto> Services { get; set; }
        public BreadcrumbDto Breadcrumb { get; set; }
    }

    public class BreadcrumbDto
    {
        public List<BreadcrumbItemDto> Items { get; set; }
    }

    public class BreadcrumbItemDto
    {
        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int Level { get; set; }
    }

    public class ServiceDetailsDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public List<PartCategoryForSelectionDto> PartCategories { get; set; }
    }

    public class PartCategoryForSelectionDto
    {
        public Guid PartCategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid ModelId { get; set; } // NEW: Vehicle model information
        public string ModelName { get; set; } // NEW: For display
        public string BrandName { get; set; } // NEW: For display
    }

    public class PartForSelectionDto
    {
        public Guid PartId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int? WarrantyMonths { get; set; } // NEW: Warranty information
        public Guid PartCategoryId { get; set; }
        public Guid ModelId { get; set; } // NEW: Vehicle model information
        public string ModelName { get; set; } // NEW: For display
        public string BrandName { get; set; } // NEW: For display
    }
}
