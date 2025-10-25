using BusinessObject;
using Dtos.RoBoard;

namespace Services
{
    public interface IOrderStatusService
    {
        Task<RoBoardColumnsDto> GetOrderStatusesByColumnsAsync();
        Task<OrderStatus?> GetOrderStatusByIdAsync(int id); // Changed from Guid to int
        Task<bool> OrderStatusExistsAsync(int id); // Changed from Guid to int
        Task<IEnumerable<Label>> GetLabelsByOrderStatusIdAsync(int orderStatusId); // Changed from Guid to int
        Task InitializeDefaultStatusesAsync();
    }
}