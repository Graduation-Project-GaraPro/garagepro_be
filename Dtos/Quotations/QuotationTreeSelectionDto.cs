using System;
using System.Collections.Generic;

namespace Dtos.Quotations
{
    /// <summary>
    /// DTO for hierarchical service category tree navigation
    /// Used by managers when creating quotations
    /// </summary>
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

    /// <summary>
    /// DTO for service selection from tree
    /// Manager selects service, then uses existing UI to add parts
    /// </summary>
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

    /// <summary>
    /// Response DTO showing the complete tree structure
    /// Used for initial load or breadcrumb navigation
    /// </summary>
    public class ServiceCategoryTreeResponseDto
    {
        public Guid? CurrentCategoryId { get; set; }
        public string CurrentCategoryName { get; set; }
        public List<ServiceCategoryTreeNodeDto> ChildCategories { get; set; }
        public List<ServiceForSelectionDto> Services { get; set; }
        public BreadcrumbDto Breadcrumb { get; set; }
    }

    /// <summary>
    /// Breadcrumb for navigation
    /// </summary>
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
}
