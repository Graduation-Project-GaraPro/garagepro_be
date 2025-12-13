using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dtos.Parts;

namespace Services.PartCategoryServices
{
    public interface IPartService
    {
        // Basic Part CRUD
        Task<IEnumerable<PartDto>> GetAllPartsAsync();
        Task<PartPagedResultDto> GetPagedPartsAsync(PaginationDto paginationDto);
        Task<IEnumerable<PartDto>> GetPartsByBranchAsync(Guid branchId);
        Task<PartPagedResultDto> GetPagedPartsByBranchAsync(Guid branchId, PaginationDto paginationDto);
        Task<PartDto> GetPartByIdAsync(Guid id);
        Task<PartPagedResultDto> SearchPartsAsync(PartSearchDto searchDto);
        Task<PartDto> CreatePartAsync(CreatePartDto dto);
        Task<EditPartDto> GetPartForEditAsync(Guid id);
        Task<EditPartDto> UpdatePartAsync(Guid id, UpdatePartDto dto);
        Task<bool> DeletePartAsync(Guid id);

        // Service-part relationship
        Task<IEnumerable<PartDto>> GetPartsForServiceAsync(Guid serviceId);
        Task<bool> UpdateServicePartCategoriesAsync(Guid serviceId, List<Guid> partCategoryIds);
        Task<ServicePartCategoryDto> GetServicePartCategoriesAsync(Guid serviceId);
    }
}
