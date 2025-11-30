using Dtos.Quotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.QuotationServices
{
    /// <summary>
    /// Service for hierarchical tree-based service and part category selection
    /// Used by managers when creating quotations
    /// </summary>
    public interface IQuotationTreeSelectionService
    {
        /// <summary>
        /// Get root-level service categories (top of the tree)
        /// </summary>
        Task<ServiceCategoryTreeResponseDto> GetRootCategoriesAsync();

        /// <summary>
        /// Get child categories and services for a specific category
        /// Drills down one level in the tree
        /// </summary>
        /// <param name="categoryId">Parent category ID</param>
        Task<ServiceCategoryTreeResponseDto> GetCategoryChildrenAsync(Guid categoryId);

        /// <summary>
        /// Get service details with associated part categories
        /// </summary>
        /// <param name="serviceId">Service ID</param>
        Task<ServiceDetailsDto> GetServiceDetailsAsync(Guid serviceId);

        /// <summary>
        /// Get all parts for a specific part category
        /// </summary>
        /// <param name="partCategoryId">Part category ID</param>
        Task<List<PartForSelectionDto>> GetPartsByCategoryAsync(Guid partCategoryId);
    }
}
