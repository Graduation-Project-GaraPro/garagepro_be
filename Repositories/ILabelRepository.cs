using BusinessObject;

namespace Repositories
{
    public interface ILabelRepository
    {
        Task<IEnumerable<Label>> GetAllAsync();
        Task<IEnumerable<Label>> GetByOrderStatusIdAsync(int orderStatusId); // Changed from Guid to int
        Task<Label?> GetByIdAsync(Guid id);
        Task<Label> CreateAsync(Label label);
        Task<Label> UpdateAsync(Label label);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}