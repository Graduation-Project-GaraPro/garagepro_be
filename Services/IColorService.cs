using BusinessObject;
using ColorEntity = BusinessObject.Color;

namespace Services
{
    public interface IColorService
    {
        Task<IEnumerable<ColorEntity>> GetActiveColorsAsync();
        Task<bool> ColorExistsAsync(Guid id);
        Task InitializeDefaultColorsAsync();
    }
}