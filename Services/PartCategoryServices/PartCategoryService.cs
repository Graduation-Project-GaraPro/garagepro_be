using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using Dtos.Parts;
using Repositories.PartRepositories;

namespace Services.PartCategoryServices
{
    public class PartCategoryService : IPartCategoryService
    {
        private readonly IPartCategoryRepository _partCategoryRepository;

        public PartCategoryService(IPartCategoryRepository partCategoryRepository)
        {
            _partCategoryRepository = partCategoryRepository;
        }

        public async Task<IEnumerable<PartCategoryDto>> GetAllPartCategoriesAsync()
        {
            var categories = await _partCategoryRepository.GetAllAsync();
            return categories.Select(MapToDto);
        }

        public async Task<PartCategoryPagedResultDto> GetPagedPartCategoriesAsync(PaginationDto paginationDto)
        {
            var (items, totalCount) = await _partCategoryRepository.GetPagedAsync(paginationDto.Page, paginationDto.PageSize);
            var totalPages = (int)Math.Ceiling(totalCount / (double)paginationDto.PageSize);

            return new PartCategoryPagedResultDto
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = paginationDto.Page,
                PageSize = paginationDto.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = paginationDto.Page > 1,
                HasNextPage = paginationDto.Page < totalPages
            };
        }

        public async Task<PartCategoryPagedResultDto> SearchPartCategoriesAsync(PartCategorySearchDto searchDto)
        {
            var (items, totalCount) = await _partCategoryRepository.SearchPartCategoriesAsync(
                searchDto.SearchTerm,
                searchDto.SortBy,
                searchDto.SortOrder,
                searchDto.Page,
                searchDto.PageSize
            );

            var totalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize);

            return new PartCategoryPagedResultDto
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = searchDto.Page > 1,
                HasNextPage = searchDto.Page < totalPages
            };
        }

        public async Task<PartCategoryDto> GetPartCategoryByIdAsync(Guid id)
        {
            var category = await _partCategoryRepository.GetByIdAsync(id);
            return category != null ? MapToDto(category) : null;
        }

        public async Task<PartCategoryDto> CreatePartCategoryAsync(CreatePartCategoryDto dto)
        {
            var category = new PartCategory
            {
                CategoryName = dto.CategoryName,
                Description = dto.Description
            };

            var created = await _partCategoryRepository.CreateAsync(category);
            return MapToDto(created);
        }

        public async Task<PartCategoryDto> UpdatePartCategoryAsync(Guid id, UpdatePartCategoryDto dto)
        {
            var existingCategory = await _partCategoryRepository.GetByIdAsync(id);
            if (existingCategory == null) return null;

            existingCategory.CategoryName = dto.CategoryName;
            existingCategory.Description = dto.Description;

            var updated = await _partCategoryRepository.UpdateAsync(existingCategory);
            return MapToDto(updated);
        }

        public async Task<bool> DeletePartCategoryAsync(Guid id)
        {
            return await _partCategoryRepository.DeleteAsync(id);
        }

        public async Task<PartCategoryWithServicesDto> GetPartCategoryWithServicesAsync(Guid id)
        {
            var categoryWithServices = await _partCategoryRepository.GetPartCategoryWithServicesAsync(id);
            return categoryWithServices;
        }

        public async Task<IEnumerable<PartCategoryWithServicesDto>> GetAllPartCategoriesWithServicesAsync()
        {
            var categoriesWithServices = await _partCategoryRepository.GetAllPartCategoriesWithServicesAsync();
            return categoriesWithServices;
        }

        private PartCategoryDto MapToDto(PartCategory category)
        {
            return new PartCategoryDto
            {
                LaborCategoryId = category.LaborCategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}