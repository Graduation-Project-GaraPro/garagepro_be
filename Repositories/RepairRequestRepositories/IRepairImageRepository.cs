using BusinessObject.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.RepairRequestRepositories
{
   public interface IRepairImageRepository
    {
        // Lấy tất cả hình của 1 RepairRequest
        Task<List<RepairImage>> GetByRepairRequestIdAsync(Guid repairRequestId);

        // Kiểm tra image có tồn tại theo URL
        Task<bool> ExistsAsync(Guid repairRequestId, string imageUrl);
        // Thêm image mới
        Task AddAsync(RepairImage image);

        // Xóa image
        void Remove(RepairImage image);
    }
}
