using BusinessObject;

namespace Repositories
{
    public interface IOrderStatusRepository
    {
        Task<IEnumerable<OrderStatus>> GetAllAsync();
        Task<OrderStatus?> GetByIdAsync(int id); // Changed from Guid to int
        Task<OrderStatus?> GetOrderStatusByIdAsync(int id); // Alias for GetByIdAsync
        Task<OrderStatus> CreateAsync(OrderStatus orderStatus);
        Task<OrderStatus> UpdateAsync(OrderStatus orderStatus);
        Task<bool> DeleteAsync(int id); // Changed from Guid to int
        Task<bool> ExistsAsync(int id); // Changed from Guid to int
        Task<IEnumerable<Label>> GetLabelsByStatusIdAsync(int statusId); // Changed from Guid to int
    }
}