using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using Dtos.Parts;
using Repositories.PartRepositories;

namespace Services.PartCategoryServices
{
    public class PartService : IPartService
    {
        private readonly IPartRepository _partRepository;

        public PartService(IPartRepository partRepository)
        {
            _partRepository = partRepository;
        }

        public async Task<IEnumerable<PartDto>> GetAllPartsAsync()
        {
            var parts = await _partRepository.GetAllAsync();
            return parts.Select(MapToDto);
        }

        public async Task<IEnumerable<PartDto>> GetPartsByBranchAsync(Guid branchId)
        {
            var parts = await _partRepository.GetByBranchIdAsync(branchId);
            return parts.Select(MapToDto);
        }

        public async Task<PartDto> GetPartByIdAsync(Guid id)
        {
            var part = await _partRepository.GetByIdAsync(id);
            return part != null ? MapToDto(part) : null;
        }

        public async Task<PartDto> CreatePartAsync(CreatePartDto dto)
        {
            var part = new Part
            {
                PartCategoryId = dto.PartCategoryId,
                BranchId = dto.BranchId,
                Name = dto.Name,
                Price = dto.Price,
                Stock = dto.Stock
            };

            var created = await _partRepository.CreateAsync(part);
            return MapToDto(created);
        }

        public async Task<PartDto> UpdatePartAsync(Guid id, UpdatePartDto dto)
        {
            var existingPart = await _partRepository.GetByIdAsync(id);
            if (existingPart == null) return null;

            existingPart.PartCategoryId = dto.PartCategoryId;
            existingPart.BranchId = dto.BranchId;
            existingPart.Name = dto.Name;
            existingPart.Price = dto.Price;
            existingPart.Stock = dto.Stock;

            var updated = await _partRepository.UpdateAsync(existingPart);
            return MapToDto(updated);
        }

        public async Task<bool> DeletePartAsync(Guid id)
        {
            return await _partRepository.DeleteAsync(id);
        }

        public async Task<PartPagedResultDto> SearchPartsAsync(PartSearchDto searchDto)
        {
            var (items, totalCount) = await _partRepository.SearchPartsAsync(
                searchDto.SearchTerm,
                searchDto.PartCategoryId,
                searchDto.BranchId,
                searchDto.MinPrice,
                searchDto.MaxPrice,
                searchDto.SortBy,
                searchDto.SortOrder,
                searchDto.Page,
                searchDto.PageSize
            );

            var totalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize);

            return new PartPagedResultDto
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

        private PartDto MapToDto(Part part)
        {
            return new PartDto
            {
                PartId = part.PartId,
                PartCategoryId = part.PartCategoryId,
                PartCategoryName = part.PartCategory?.CategoryName ?? "",
                BranchId = part.BranchId,
                BranchName = part.Branch?.BranchName ?? "",
                Name = part.Name,
                Price = part.Price,
                Stock = part.Stock,
                CreatedAt = part.CreatedAt,
                UpdatedAt = part.UpdatedAt
            };
        }
    }
}
