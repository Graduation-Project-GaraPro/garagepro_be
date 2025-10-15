using BusinessObject.Manager;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class FeedBackService : IFeedBackService
    {
        private readonly IFeedBackRepository _feedbackRepository;
        public FeedBackService(IFeedBackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }
        public async Task AddAsync(FeedBack feedback)
        {
            await _feedbackRepository.AddAsync(feedback);
        }

        public async Task DeleteAsync(Guid feedbackId)
        {
            await _feedbackRepository.DeleteAsync(feedbackId);
        }

        public async Task<List<FeedBack>> GetAllAsync()
        {
            return await _feedbackRepository.GetAllAsync();
        }

        public async Task<FeedBack> GetByIdAsync(Guid feedbackId)
        {
            return await _feedbackRepository.GetByIdAsync(feedbackId);
        }

        public async Task<List<FeedBack>> GetByRepairOrderIdAsync(Guid repairOrderId)
        {
            return await _feedbackRepository.GetByRepairOrderIdAsync(repairOrderId);
        }

        public async Task<List<FeedBack>> GetByUserIdAsync(string userId)
        {
            return await _feedbackRepository.GetByUserIdAsync(userId);
        }

        public async Task UpdateAsync(FeedBack feedback)
        {
            await _feedbackRepository.UpdateAsync(feedback);
        }
    }
}
