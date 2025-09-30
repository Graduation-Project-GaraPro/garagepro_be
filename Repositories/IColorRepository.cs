using BusinessObject;
using ColorEntity = BusinessObject.Color;

namespace Repositories
{
    public interface IColorRepository
    {
        Task<IEnumerable<ColorEntity>> GetActiveColorsAsync();
        Task<bool> ExistsAsync(Guid id);
        Task InitializeDefaultColorsAsync();
    }
}