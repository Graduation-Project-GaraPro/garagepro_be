using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;
using Dtos.Parts;

namespace Repositories.PartRepositories
{
    public interface IPartCategoryRepository
    {
        Task<PartCategory> GetByIdAsync(Guid id);
        Task<IEnumerable<PartCategory>> GetAllAsync();
        Task<(IEnumerable<PartCategory> items, int totalCount)> GetPagedAsync(int page, int pageSize);
        IQueryable<PartCategory> Query();
        Task<IEnumerable<PartCategory>> GetAllWithPartsAsync();

        Task<(IEnumerable<PartCategory> items, int totalCount)> SearchPartCategoriesAsync(
            string searchTerm, Guid? modelId, string modelName, Guid? brandId, string brandName, string sortBy, string sortOrder, int page, int pageSize);
        Task<PartCategory> CreateAsync(PartCategory category);
        Task<PartCategory> UpdateAsync(PartCategory category);
        Task<bool> DeleteAsync(Guid id);
        Task<PartCategoryWithServicesDto> GetPartCategoryWithServicesAsync(Guid id);
        Task<IEnumerable<PartCategoryWithServicesDto>> GetAllPartCategoriesWithServicesAsync();
    }
}