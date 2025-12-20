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
        /// Get service details with associated part categories (model-specific)
        /// </summary>
        /// <param name="serviceId">Service ID</param>
        /// <param name="modelId">Vehicle model ID for model-specific part categories</param>
        Task<ServiceDetailsDto> GetServiceDetailsAsync(Guid serviceId, Guid? modelId = null);

        /// <summary>
        /// Get all parts for a specific part category
        /// </summary>
        /// <param name="partCategoryId">Part category ID</param>
        /// <param name="modelId">Optional vehicle model ID to filter parts</param>
        Task<List<PartForSelectionDto>> GetPartsByCategoryAsync(Guid partCategoryId, Guid? modelId = null);

        /// <summary>
        /// Get parts by vehicle model and category name (for model-specific categories)
        /// </summary>
        /// <param name="modelId">Vehicle model ID</param>
        /// <param name="categoryName">Category name</param>
        Task<List<PartForSelectionDto>> GetPartsByModelAndCategoryAsync(Guid modelId, string categoryName);
    }
}
