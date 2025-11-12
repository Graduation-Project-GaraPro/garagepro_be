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
        Task<IEnumerable<RepairRequest>> GetAllAsync();
        Task<IEnumerable<RepairRequest>> GetByUserIdAsync(string userId);
        Task<IEnumerable<ManagerRepairRequestDto>> GetForManagerAsync(); // New method for managers
        Task<ManagerRepairRequestDto> GetManagerRequestByIdAsync(Guid id); // New method for getting single request for manager
        Task<RepairRequestDto> GetByIdAsync(Guid id);
        Task<RPDetailDto> GetByIdDetailsAsync(Guid id);
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
        Task<RepairRequestDto> CreateRepairWithImageRequestAsync(CreateRepairRequestWithImageDto dto, string userId);
        Task<IEnumerable<RequestServiceDto>> GetServicesAsync(Guid repairRequestId);
        Task<RequestServiceDto> AddServiceAsync(RequestServiceDto dto);
        Task<bool> DeleteServiceAsync(Guid requestServiceId);
    }
}