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

        Task<IEnumerable<RepairRequest>> GetAllAsync();
        Task<IEnumerable<RepairRequest>> GetByUserIdAsync(string userId);
        Task<IEnumerable<ManagerRepairRequestDto>> GetForManagerAsync(); // New method for managers
        Task<ManagerRepairRequestDto> GetManagerRequestByIdAsync(Guid id); // New method for getting single request for manager        
       
        Task<IEnumerable<ManagerRepairRequestDto>> GetForManagerByBranchAsync(Guid branchId); 
              

        Task<RPDetailDto> GetByIdDetailsAsync(Guid id);

        Task<bool> CustomerCancelRepairRequestAsync(Guid requestId, string userId);
        
        // Manager can cancel on behalf of customer (within 30 minutes before RequestDate)
        Task<bool> ManagerCancelRepairRequestAsync(Guid requestId, string managerId);

        Task<IReadOnlyList<SlotAvailabilityDto>> GetArrivalAvailabilityAsync(Guid branchId, DateOnly date);

        Task<RepairRequestDto> CreateRepairRequestAsync(CreateRequestDto dto, string userId);
        Task<RepairRequestDto> UpdateRepairRequestAsync(Guid requestId, UpdateRepairRequestDto dto,string userId);
        Task<bool> DeleteRepairRequestAsync(Guid id);

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