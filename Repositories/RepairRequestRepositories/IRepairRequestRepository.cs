using BusinessObject.Customers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Customers
{
    public interface IRepairRequestRepository
    {
        Task<IEnumerable<RepairRequest>> GetAllAsync();
        Task<IEnumerable<RepairRequest>> GetByUserIdAsync(String userId);
        Task<RepairRequest> GetByIdAsync(Guid id);
        Task<RepairRequest> GetByIdWithDetailsAsync(Guid id); // New method for managers
        Task<RepairRequest> AddAsync(RepairRequest repairRequest);
        Task<RepairRequest> UpdateAsync(RepairRequest repairRequest);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<IEnumerable<RepairImage>> GetImagesAsync(Guid requestId);
        Task<RepairImage> AddImageAsync(RepairImage image);
        Task<bool> DeleteImageAsync(Guid imageId);
    }
}