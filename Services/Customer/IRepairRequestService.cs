using Dtos.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Customer
{
    public interface IRepairRequestService
    {
        Task<IEnumerable<RepairRequestDto>> GetAllAsync();
        Task<IEnumerable<RepairRequestDto>> GetByUserIdAsync(string userId);
        Task<RepairRequestDto> GetByIdAsync(Guid id);
        Task<RepairRequestDto> CreateRepairRequestAsync(CreateRequestDto dto, string userId);
        Task<RepairRequestDto> UpdateRepairRequestAsync(Guid id, UpdateRepairRequestDto dto);
        Task<bool> DeleteRepairRequestAsync(Guid id);

        // For RepairImages
        Task<IEnumerable<RequestImagesDto>> GetImagesAsync(Guid repairRequestId);
        Task<RequestImagesDto> AddImageAsync(RequestImagesDto dto);
        Task<bool> DeleteImageAsync(Guid imageId);

        // Optional: for parts and services
        //Task<IEnumerable<RequestPartDto>> GetPartsAsync(Guid repairRequestId);
        //Task<RequestPartDto> AddPartAsync(RequestPartDto dto);
        //Task<bool> DeletePartAsync(Guid partId);

        Task<IEnumerable<RequestServiceDto>> GetServicesAsync(Guid repairRequestId);
        Task<RequestServiceDto> AddServiceAsync(RequestServiceDto dto);
        Task<bool> DeleteServiceAsync(Guid requestServiceId);
    }
}
