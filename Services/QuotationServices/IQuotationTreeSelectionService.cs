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


    }
}
