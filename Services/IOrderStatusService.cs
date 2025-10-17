using BusinessObject;
using Dtos.RoBoard;

namespace Services
{
    public interface IOrderStatusService
    {
        Task<RoBoardColumnsDto> GetOrderStatusesByColumnsAsync();
        Task<OrderStatus?> GetOrderStatusByIdAsync(Guid id);
        Task<bool> OrderStatusExistsAsync(Guid id);
        Task<IEnumerable<Label>> GetLabelsByOrderStatusIdAsync(Guid orderStatusId);
        Task InitializeDefaultStatusesAsync();
    }
}