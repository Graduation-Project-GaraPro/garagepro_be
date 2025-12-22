using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;
using Dtos.Parts;

namespace Repositories.PartRepositories
{
    public interface IPartRepository
    {
        // Basic CRUD methods
        Task<Part> GetByIdAsync(Guid id);
        Task<IEnumerable<Part>> GetAllAsync();
        Task<(IEnumerable<Part> items, int totalCount)> GetPagedAsync(int page, int pageSize);
        Task<IEnumerable<Part>> GetByBranchIdAsync(Guid branchId);
        Task<(IEnumerable<Part> items, int totalCount)> GetPagedByBranchAsync(Guid branchId, int page, int pageSize);
        Task<Part> CreateAsync(Part part);
        Task<Part> UpdateAsync(Part part);
        Task<bool> DeleteAsync(Guid id);
        Task<(IEnumerable<Part> Items, int TotalCount)> SearchPartsAsync(
            string searchTerm,
            Guid? partCategoryId,
            Guid? branchId,
            Guid? modelId,
            string modelName,
            Guid? brandId,
            string brandName,
            string categoryName,
            decimal? minPrice,
            decimal? maxPrice,
            string sortBy,
            string sortOrder,
            int page,
            int pageSize);

        // Service-part relationship methods
        Task<IEnumerable<Part>> GetPartsForServiceAsync(Guid serviceId);
        Task<bool> UpdateServicePartCategoriesAsync(Guid serviceId, List<Guid> partCategoryIds);
        Task<ServicePartCategoryDto> GetServiceWithPartCategoriesAsync(Guid serviceId);
    }
}