using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dtos.Parts;

namespace Services.PartCategoryServices
{
    public interface IPartCategoryService
    {
        Task<IEnumerable<PartCategoryWithPartsDto>> GetAllWithPartsAsync();
        Task<PartCategoryWithPartsDto?> GetByIdWithPartsAsync(Guid id);
        Task<IEnumerable<PartCategoryWithPartsDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? categoryName);
    }

}
