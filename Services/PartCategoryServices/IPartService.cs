using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;
using Dtos.Parts;

namespace Services.PartCategoryServices
{
    public interface IPartService
    {
        Task<IEnumerable<PartDto>> GetAllPartsAsync();
        Task<PartDto> GetPartByIdAsync(Guid id);
        //Task<IEnumerable<PartByServiceDto>> GetPartsByServiceIdAsync(Guid serviceId);
        Task<PartDto> CreatePartAsync(CreatePartDto dto);
        Task<PartDto> UpdatePartAsync(Guid id, UpdatePartDto dto);
        Task<bool> DeletePartAsync(Guid id);
    }
}