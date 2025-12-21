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

        public async Task<PartPagedResultDto> GetPagedPartsAsync(PaginationDto paginationDto)
        {
            var (items, totalCount) = await _partRepository.GetPagedAsync(paginationDto.Page, paginationDto.PageSize);
            var totalPages = (int)Math.Ceiling(totalCount / (double)paginationDto.PageSize);

            return new PartPagedResultDto
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

        public async Task<IEnumerable<PartDto>> GetPartsByBranchAsync(Guid branchId)
        {
            var parts = await _partRepository.GetByBranchIdAsync(branchId);
            return parts.Select(MapToDto);
        }

        public async Task<PartPagedResultDto> GetPagedPartsByBranchAsync(Guid branchId, PaginationDto paginationDto)
        {
            var (items, totalCount) = await _partRepository.GetPagedByBranchAsync(branchId, paginationDto.Page, paginationDto.PageSize);
            var totalPages = (int)Math.Ceiling(totalCount / (double)paginationDto.PageSize);

            return new PartPagedResultDto
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

        public async Task<PartDto> GetPartByIdAsync(Guid id)
        {
            var part = await _partRepository.GetByIdAsync(id);
            return part != null ? MapToDto(part) : null;
        }

        public async Task<EditPartDto> GetPartForEditAsync(Guid id)
        {
            var part = await _partRepository.GetByIdAsync(id);
            return part != null ? MapToEditDto(part) : null;
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

        public async Task<EditPartDto> UpdatePartAsync(Guid id, UpdatePartDto dto)
        {
            var existingPart = await _partRepository.GetByIdAsync(id);
            if (existingPart == null) return null;

            existingPart.PartCategoryId = dto.PartCategoryId;
            // BranchId is NOT updated - it remains unchanged to preserve original branch assignment
            existingPart.Name = dto.Name;
            existingPart.Price = dto.Price;
            existingPart.Stock = dto.Stock;

            var updated = await _partRepository.UpdateAsync(existingPart);
            return MapToEditDto(updated);
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
                searchDto.ModelId,
                searchDto.ModelName,
                searchDto.BrandId,
                searchDto.BrandName,
                searchDto.CategoryName,
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

        public async Task<IEnumerable<PartDto>> GetPartsForServiceAsync(Guid serviceId)
        {
            var parts = await _partRepository.GetPartsForServiceAsync(serviceId);
            return parts.Select(MapToDto);
        }

        public async Task<bool> UpdateServicePartCategoriesAsync(Guid serviceId, List<Guid> partCategoryIds)
        {
            return await _partRepository.UpdateServicePartCategoriesAsync(serviceId, partCategoryIds);
        }

        public async Task<ServicePartCategoryDto> GetServicePartCategoriesAsync(Guid serviceId)
        {
            var serviceWithCategories = await _partRepository.GetServiceWithPartCategoriesAsync(serviceId);
            return serviceWithCategories;
        }

        private PartDto MapToDto(Part part)
        {
            // Calculate stock from PartInventory instead of Part.Stock
            var totalStock = part.PartInventories?.Sum(pi => pi.Stock) ?? 0;
            
            // If filtering by branch, show only that branch's stock
            var branchStock = part.BranchId.HasValue 
                ? part.PartInventories?.FirstOrDefault(pi => pi.BranchId == part.BranchId.Value)?.Stock ?? 0
                : totalStock;

            return new PartDto
            {
                PartId = part.PartId,
                PartCategoryId = part.PartCategoryId,
                PartCategoryName = part.PartCategory?.CategoryName ?? "",
                BranchId = part.BranchId,
                BranchName = part.Branch?.BranchName ?? "",
                Name = part.Name,
                Price = part.Price,
                Stock = branchStock, // ✅ Now using PartInventory data
                ModelId = part.PartCategory?.ModelId ?? Guid.Empty,
                ModelName = part.PartCategory?.VehicleModel?.ModelName ?? "",
                BrandName = part.PartCategory?.VehicleModel?.Brand?.BrandName ?? "",
                BrandId = part.PartCategory?.VehicleModel?.BrandID ?? Guid.Empty,
                CreatedAt = part.CreatedAt,
                UpdatedAt = part.UpdatedAt
            };
        }

        private EditPartDto MapToEditDto(Part part)
        {
            return new EditPartDto
            {
                PartId = part.PartId,
                PartCategoryId = part.PartCategoryId,
                PartCategoryName = part.PartCategory?.CategoryName ?? "",
                Name = part.Name,
                Price = part.Price,
                Stock = part.Stock,
                CreatedAt = part.CreatedAt,
                UpdatedAt = part.UpdatedAt
            };
        }
    }
}
