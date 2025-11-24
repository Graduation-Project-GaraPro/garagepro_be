using BusinessObject.Customers;
using Dtos.Customers;
using Dtos.RepairOrder;
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
        Task<IEnumerable<ManagerRepairRequestDto>> GetForManagerAsync(); 
        Task<IEnumerable<ManagerRepairRequestDto>> GetForManagerByBranchAsync(Guid branchId); 
        Task<ManagerRepairRequestDto> GetManagerRequestByIdAsync(Guid id);        
        Task<RPDetailDto> GetByIdDetailsAsync(Guid id);

        Task<bool> CustomerCancelRepairRequestAsync(Guid requestId, string userId);

        Task<IReadOnlyList<SlotAvailabilityDto>> GetArrivalAvailabilityAsync(Guid branchId, DateOnly date);

        Task<RepairRequestDto> CreateRepairRequestAsync(CreateRequestDto dto, string userId);
        Task<RepairRequestDto> UpdateRepairRequestAsync(Guid requestId, UpdateRepairRequestDto dto,string userId);
        Task<bool> DeleteRepairRequestAsync(Guid id);

        // Approval and rejection methods
        Task<bool> ApproveRepairRequestAsync(Guid requestId);
        Task<bool> RejectRepairRequestAsync(Guid requestId);

        // Conversion method
        Task<RepairOrderDto> ConvertToRepairOrderAsync(Guid requestId, CreateRoFromRequestDto dto);

        Task<object> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            Guid? vehicleId = null,
            RepairRequestStatus? status = null,
            Guid? branchId = null,
            string? userId = null);

        // For RepairImages
       
        Task<RepairRequestDto> CreateRepairWithImageRequestAsync(CreateRepairRequestWithImageDto dto, string userId);
        Task<IEnumerable<RequestServiceDto>> GetServicesAsync(Guid repairRequestId);
        Task<RequestServiceDto> AddServiceAsync(RequestServiceDto dto);
        Task<bool> DeleteServiceAsync(Guid requestServiceId);
    }
}