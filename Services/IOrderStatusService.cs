using BusinessObject;

namespace Services
{
    public interface IOrderStatusService
    {
        Task<object> GetOrderStatusesByColumnsAsync();
        Task<OrderStatus?> GetOrderStatusByIdAsync(Guid id);
        Task<bool> OrderStatusExistsAsync(Guid id);
        Task<IEnumerable<Label>> GetLabelsByOrderStatusIdAsync(Guid orderStatusId);
        Task InitializeDefaultStatusesAsync();
    }
}