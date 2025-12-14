using BusinessObject;
using Repositories;

namespace Services
{
    public class LabelService : ILabelService
    {
        private readonly ILabelRepository _labelRepository;
        private readonly IOrderStatusRepository _orderStatusRepository;

        public LabelService(ILabelRepository labelRepository, IOrderStatusRepository orderStatusRepository)
        {
            _labelRepository = labelRepository;
            _orderStatusRepository = orderStatusRepository;
        }

        public async Task<IEnumerable<Label>> GetAllLabelsAsync()
        {
            return await _labelRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Label>> GetLabelsByOrderStatusIdAsync(int orderStatusId) // Changed from Guid to int
        {
            // Validate that order status exists
            if (!await _orderStatusRepository.ExistsAsync(orderStatusId))
                throw new KeyNotFoundException($"Order status with ID {orderStatusId} not found");

            return await _labelRepository.GetByOrderStatusIdAsync(orderStatusId);
        }

        public async Task<IEnumerable<Label>> GetDefaultLabelsByOrderStatusIdAsync(int orderStatusId)
        {
            // Validate that order status exists
            if (!await _orderStatusRepository.ExistsAsync(orderStatusId))
                throw new KeyNotFoundException($"Order status with ID {orderStatusId} not found");

            return await _labelRepository.GetDefaultByOrderStatusIdAsync(orderStatusId);
        }

        public async Task<Label?> GetLabelByIdAsync(Guid id)
        {
            return await _labelRepository.GetByIdAsync(id);
        }

        public async Task<Label> CreateLabelAsync(Label label)
        {
            // Business logic validation
            if (string.IsNullOrWhiteSpace(label.LabelName))
                throw new ArgumentException("Label name is required", nameof(label.LabelName));

            if (label.LabelName.Length > 100)
                throw new ArgumentException("Label name cannot exceed 100 characters", nameof(label.LabelName));

            if (label.Description != null && label.Description.Length > 500)
                throw new ArgumentException("Description cannot exceed 500 characters", nameof(label.Description));

            // Validate that order status exists
            if (!await _orderStatusRepository.ExistsAsync(label.OrderStatusId))
                throw new KeyNotFoundException($"Order status with ID {label.OrderStatusId} not found");

            // Set creation values
            label.LabelId = Guid.NewGuid();

            return await _labelRepository.CreateAsync(label);
        }

        public async Task<Label> UpdateLabelAsync(Label label)
        {
            // Check if label exists without tracking
            if (!await _labelRepository.ExistsAsync(label.LabelId))
                throw new KeyNotFoundException($"Label with ID {label.LabelId} not found");

            // Business logic validation
            if (string.IsNullOrWhiteSpace(label.LabelName))
                throw new ArgumentException("Label name is required", nameof(label.LabelName));

            if (label.LabelName.Length > 100)
                throw new ArgumentException("Label name cannot exceed 100 characters", nameof(label.LabelName));

            if (label.Description != null && label.Description.Length > 500)
                throw new ArgumentException("Description cannot exceed 500 characters", nameof(label.Description));

            // Validate that order status exists
            if (!await _orderStatusRepository.ExistsAsync(label.OrderStatusId))
                throw new KeyNotFoundException($"Order status with ID {label.OrderStatusId} not found");

            return await _labelRepository.UpdateAsync(label);
        }

        public async Task<bool> DeleteLabelAsync(Guid id)
        {
            // Check if label exists
            var existingLabel = await _labelRepository.GetByIdAsync(id);
            if (existingLabel == null)
                return false;

            // Business logic: Labels can be safely deleted as they don't have direct dependencies
            // The RepairOrder-OrderStatus-Label relationship structure allows this
            return await _labelRepository.DeleteAsync(id);
        }

        public async Task<bool> LabelExistsAsync(Guid id)
        {
            return await _labelRepository.ExistsAsync(id);
        }
    }
}