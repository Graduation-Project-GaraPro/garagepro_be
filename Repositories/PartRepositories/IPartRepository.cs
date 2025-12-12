using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;

namespace Repositories.PartRepositories
{
    public interface IPartRepository
    {
        Task<Part> GetByIdAsync(Guid id);
        Task<IEnumerable<Part>> GetAllAsync();
        Task<IEnumerable<Part>> GetByBranchIdAsync(Guid branchId);
        Task<(IEnumerable<Part> Items, int TotalCount)> SearchPartsAsync(
            string searchTerm,
            Guid? partCategoryId,
            Guid? branchId,
            decimal? minPrice,
            decimal? maxPrice,
            string sortBy,
            string sortOrder,
            int page,
            int pageSize);
        Task<Part> CreateAsync(Part part);
        Task<Part> UpdateAsync(Part part);
        Task<bool> DeleteAsync(Guid id);
    }
}
