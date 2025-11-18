using BusinessObject.RequestEmergency;
using Dtos.Emergency.Dtos.Emergency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EmergencyRequestRepositories
{
    public interface  IPriceEmergencyRepositories
    {
        Task<PriceEmergency> GetLatestPriceAsync();        // Lấy giá đang được áp dụng
        Task AddPriceAsync(PriceEmergencyDto price);          // Thêm giá mới
        Task<IEnumerable<PriceEmergency>> GetAllPricesAsync(); // Lịch sử giá
    }
}
