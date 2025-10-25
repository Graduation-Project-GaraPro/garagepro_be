using BusinessObject;
using Repositories;
using Dtos.RoBoard; // Add this using statement
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class OrderStatusService : IOrderStatusService
    {
        private readonly IOrderStatusRepository _orderStatusRepository;

        // Predefined status names for the 3 columns
        private readonly Dictionary<string, string> _defaultStatuses = new Dictionary<string, string>
        {
            { "Pending", "Orders waiting to be processed" },
            { "In Progress", "Orders currently being worked on" },
            { "Completed", "Orders that have been finished" }
        };

        public OrderStatusService(IOrderStatusRepository orderStatusRepository)
        {
            _orderStatusRepository = orderStatusRepository;
        }

        public async Task<RoBoardColumnsDto> GetOrderStatusesByColumnsAsync()
        {
            var allStatuses = await _orderStatusRepository.GetAllAsync();
            
            // Group statuses into 3 columns based on predefined names
            var result = new RoBoardColumnsDto
            {
                Pending = allStatuses.Where(s => s.StatusName == "Pending")
                    .Select(s => new RoBoardColumnDto
                    {
                        OrderStatusId = s.OrderStatusId,
                        StatusName = s.StatusName,
                        RepairOrderCount = s.RepairOrders?.Count ?? 0
                    }).ToList(),
                InProgress = allStatuses.Where(s => s.StatusName == "In Progress")
                    .Select(s => new RoBoardColumnDto
                    {
                        OrderStatusId = s.OrderStatusId,
                        StatusName = s.StatusName,
                        RepairOrderCount = s.RepairOrders?.Count ?? 0
                    }).ToList(),
                Completed = allStatuses.Where(s => s.StatusName == "Completed")
                    .Select(s => new RoBoardColumnDto
                    {
                        OrderStatusId = s.OrderStatusId,
                        StatusName = s.StatusName,
                        RepairOrderCount = s.RepairOrders?.Count ?? 0
                    }).ToList()
            };

            return result;
        }

        public async Task<OrderStatus?> GetOrderStatusByIdAsync(int id) // Changed from Guid to int
        {
            return await _orderStatusRepository.GetByIdAsync(id);
        }

        public async Task<bool> OrderStatusExistsAsync(int id) // Changed from Guid to int
        {
            return await _orderStatusRepository.ExistsAsync(id);
        }

        public async Task<IEnumerable<Label>> GetLabelsByOrderStatusIdAsync(int orderStatusId) // Changed from Guid to int
        {
            // Validate that order status exists
            if (!await _orderStatusRepository.ExistsAsync(orderStatusId))
                throw new KeyNotFoundException($"Order status with ID {orderStatusId} not found");

            return await _orderStatusRepository.GetLabelsByStatusIdAsync(orderStatusId);
        }

        public async Task InitializeDefaultStatusesAsync()
        {
            var existingStatuses = await _orderStatusRepository.GetAllAsync();
            
            // Check if we already have the default statuses with the correct names
            var pendingStatus = existingStatuses.FirstOrDefault(s => s.StatusName == "Pending");
            var inProgressStatus = existingStatuses.FirstOrDefault(s => s.StatusName == "In Progress");
            var completedStatus = existingStatuses.FirstOrDefault(s => s.StatusName == "Completed");
            
            // If any are missing, we'll add them without specifying IDs (let the database generate them)
            if (pendingStatus == null)
            {
                var newStatus = new OrderStatus
                {
                    StatusName = "Pending"
                };
                await _orderStatusRepository.CreateAsync(newStatus);
            }
            
            if (inProgressStatus == null)
            {
                var newStatus = new OrderStatus
                {
                    StatusName = "In Progress"
                };
                await _orderStatusRepository.CreateAsync(newStatus);
            }
            
            if (completedStatus == null)
            {
                var newStatus = new OrderStatus
                {
                    StatusName = "Completed"
                };
                await _orderStatusRepository.CreateAsync(newStatus);
            }
        }
    }
}