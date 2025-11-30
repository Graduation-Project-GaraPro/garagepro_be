using AutoMapper;
using BusinessObject.Manager;
using Dtos.FeedBacks;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class FeedBackService : IFeedBackService
    {
        private readonly IFeedBackRepository _feedbackRepository;
        private readonly IRepairOrderRepository _repairOrderRepository;
        private readonly IMapper _mapper;
        public FeedBackService(IFeedBackRepository feedbackRepository, IRepairOrderRepository repairOrderRepository, IMapper mapper)
        {
            _feedbackRepository = feedbackRepository;
            _repairOrderRepository = repairOrderRepository;
            _mapper = mapper;
        }

        public async Task<FeedBackReadDto> CreateFeedbackAsync(FeedBackCreateDto dto, string userId)
        {
            var order = await _repairOrderRepository.GetByIdArchivedAsync(dto.RepairOrderId);
            if (order == null)
                throw new Exception("Repair order not found.");

            if (order.UserId != userId)
                throw new Exception("You are not allowed to give feedback for this order.");
            /// ch
            if (order.OrderStatus.OrderStatusId != 3) 
                throw new Exception("Feedback can only be given after the order is completed.");
            if(order.PaidStatus != BusinessObject.Enums.PaidStatus.Paid )
                throw new Exception("Feedback can only be given after the order is paid.");

            //var existingFeedback = await _feedbackRepository
            //    .GetFeedbackByOrderIdAsync(dto.RepairOrderId);
            //if (existingFeedback != null)
            //    throw new Exception("Feedback for this order already exists.");

            var feedback = new FeedBack
            {
                FeedBackId = Guid.NewGuid(),
                UserId = userId,
                RepairOrderId = dto.RepairOrderId,
                Description = dto.Description,
                Rating = dto.Rating,
                CreatedAt = DateTime.UtcNow,
               
            };

            await _feedbackRepository.AddAsync(feedback);
           // await _feedbackRepository.SaveChangesAsync();

            return _mapper.Map<FeedBackReadDto>(feedback);
          
        }

        public async Task<bool> DeleteFeedbackAsync(Guid feedbackId, string userId)
        {
            var feedback = await _feedbackRepository.GetByIdAsync(feedbackId);
            if (feedback == null)
                throw new Exception("Feedback not found.");

            if (feedback.UserId != userId)
                throw new Exception("You are not allowed to delete this feedback.");

            await _feedbackRepository.DeleteAsync(feedback.FeedBackId);

            return true;
        }

        public async Task<IEnumerable<FeedBackReadDto>> GetAllAsync()
        {
            var feedbacks = await _feedbackRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<FeedBackReadDto>>(feedbacks);
        }

        public Task<IEnumerable<FeedBackReadDto>> GetByRepairOrderIdAsync(Guid repairOrderId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<FeedBackReadDto>> GetFeedbacksByBranchIdAsync(Guid branchId)
        {
            var feedbacks =await  _feedbackRepository.GetFeedbackByBranchIdAsync(branchId);
           
            return _mapper.Map<IEnumerable<FeedBackReadDto>>(feedbacks);

            
        }

        public async Task<IEnumerable<FeedBackReadDto>> GetFeedbacksByUserIdAsync(string userId)
        {
            var feedbacks =  _feedbackRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<FeedBackReadDto>>(feedbacks);
        }

        public async Task<FeedBackReadDto> UpdateFeedbackAsync(Guid feedbackId, FeedBackUpdateDto dto, string userId)
        {
            var feedback = await _feedbackRepository.GetByIdAsync(feedbackId);
            if (feedback == null)
                throw new Exception("Feedback not found.");

            if (feedback.UserId != userId)
                throw new Exception("You are not allowed to update this feedback.");

            feedback.Description = dto.Description ?? feedback.Description;
            feedback.Rating = dto.Rating ?? feedback.Rating;
            feedback.UpdatedAt = DateTime.UtcNow;

            await _feedbackRepository.UpdateAsync(feedback);
           // await _feedbackRepository.SaveChangesAsync();

            return _mapper.Map<FeedBackReadDto>(feedback);
        }
    }
}
