using BusinessObject.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IFeedBackRepository
    {
        Task<List<FeedBack>> GetAllAsync();
        Task<FeedBack> GetByIdAsync(Guid feedbackId);
        Task AddAsync(FeedBack feedback);
        Task UpdateAsync(FeedBack feedback);
        Task DeleteAsync(Guid feedbackId);
        Task<List<FeedBack>> GetByUserIdAsync(string userId);
        Task<List<FeedBack>> GetByRepairOrderIdAsync(Guid repairOrderId);
    }
}
