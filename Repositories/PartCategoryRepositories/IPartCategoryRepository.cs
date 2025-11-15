using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;

namespace Repositories.PartCategoryRepositories
{
    public interface IPartCategoryRepository
    {
        Task<IEnumerable<PartCategory>> GetAllWithPartsAsync();
        Task<PartCategory?> GetByIdWithPartsAsync(Guid id);
        Task<IEnumerable<PartCategory>> GetPagedAsync(int pageNumber, int pageSize, string? categoryName);
        IQueryable<PartCategory> Query();
    }
}
