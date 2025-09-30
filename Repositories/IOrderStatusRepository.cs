using BusinessObject;

namespace Repositories
{
    public interface IOrderStatusRepository
    {
        Task<IEnumerable<OrderStatus>> GetAllAsync();
        Task<OrderStatus?> GetByIdAsync(Guid id);
        Task<OrderStatus> CreateAsync(OrderStatus orderStatus);
        Task<OrderStatus> UpdateAsync(OrderStatus orderStatus);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<IEnumerable<Label>> GetLabelsByStatusIdAsync(Guid statusId);
    }
}