using BusinessObject;

namespace Repositories
{
    public interface ILabelRepository
    {
        Task<IEnumerable<Label>> GetAllAsync();
        Task<IEnumerable<Label>> GetByOrderStatusIdAsync(Guid orderStatusId);
        Task<Label?> GetByIdAsync(Guid id);
        Task<Label> CreateAsync(Label label);
        Task<Label> UpdateAsync(Label label);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}