using BusinessObject.Manager;
using Dtos.FeedBacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IFeedBackService
    {
        Task<IEnumerable<FeedBackReadDto>> GetAllAsync();
        Task<FeedBackReadDto> CreateFeedbackAsync(FeedBackCreateDto dto, string userId);
        Task<FeedBackReadDto> UpdateFeedbackAsync(Guid feedbackId,FeedBackUpdateDto dto, string userId);
        Task<bool> DeleteFeedbackAsync(Guid feedbackId, string userId);
        Task<IEnumerable<FeedBackReadDto>> GetFeedbacksByBranchIdAsync(Guid branchId);
        Task<IEnumerable<FeedBackReadDto>> GetFeedbacksByUserIdAsync(string userId);
        Task<IEnumerable<FeedBackReadDto>> GetByRepairOrderIdAsync(Guid repairOrderId);
    }
}
