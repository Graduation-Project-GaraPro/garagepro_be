using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dtos.Parts;

namespace Services.PartCategoryServices
{
    public interface IPartCategoryService
    {
        Task<IEnumerable<PartCategoryDto>> GetAllPartCategoriesAsync();
        Task<PartCategoryPagedResultDto> GetPagedPartCategoriesAsync(PaginationDto paginationDto);
        Task<PartCategoryPagedResultDto> SearchPartCategoriesAsync(PartCategorySearchDto searchDto);
        Task<PartCategoryDto> GetPartCategoryByIdAsync(Guid id);
        Task<IEnumerable<PartCategoryWithPartsDto>> GetAllWithPartsAsync();
        Task<PartCategoryDto> CreatePartCategoryAsync(CreatePartCategoryDto dto);
        Task<PartCategoryDto> UpdatePartCategoryAsync(Guid id, UpdatePartCategoryDto dto);
        Task<bool> DeletePartCategoryAsync(Guid id);
        Task<PartCategoryWithServicesDto> GetPartCategoryWithServicesAsync(Guid id);
        Task<IEnumerable<PartCategoryWithServicesDto>> GetAllPartCategoriesWithServicesAsync();
    }
}