using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dtos.Parts;

namespace Services.PartCategoryServices
{
    public interface IPartService
    {
        Task<IEnumerable<PartDto>> GetAllPartsAsync();
        Task<IEnumerable<PartDto>> GetPartsByBranchAsync(Guid branchId);
        Task<PartDto> GetPartByIdAsync(Guid id);
        Task<PartPagedResultDto> SearchPartsAsync(PartSearchDto searchDto);
        Task<PartDto> CreatePartAsync(CreatePartDto dto);
        Task<PartDto> UpdatePartAsync(Guid id, UpdatePartDto dto);
        Task<bool> DeletePartAsync(Guid id);
    }
}
