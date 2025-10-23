using BusinessObject.Customers;
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
        Task<RPDetailDto> GetByIdAsync(Guid id);
        Task<RepairRequestDto> CreateRepairRequestAsync(CreateRequestDto dto, string userId);
        Task<RepairRequestDto> UpdateRepairRequestAsync(Guid requestId, UpdateRepairRequestDto dto,string userId);
        Task<bool> DeleteRepairRequestAsync(Guid id);


        Task<object> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            Guid? vehicleId = null,
            RepairRequestStatus? status = null,
            Guid? branchId = null,
            string? userId = null);

        // For RepairImages
        Task<IEnumerable<RequestImagesDto>> GetImagesAsync(Guid repairRequestId);
        Task<RequestImagesDto> AddImageAsync(RequestImagesDto dto);
        Task<bool> DeleteImageAsync(Guid imageId);

        // Optional: for parts and services
        //Task<IEnumerable<RequestPartDto>> GetPartsAsync(Guid repairRequestId);
        //Task<RequestPartDto> AddPartAsync(RequestPartDto dto);
        //Task<bool> DeletePartAsync(Guid partId);
        Task<RepairRequestDto> CreateRepairWithImageRequestAsync(CreateRepairRequestWithImageDto dto, string userId);
        Task<IEnumerable<RequestServiceDto>> GetServicesAsync(Guid repairRequestId);
        Task<RequestServiceDto> AddServiceAsync(RequestServiceDto dto);
        Task<bool> DeleteServiceAsync(Guid requestServiceId);
    }
}
