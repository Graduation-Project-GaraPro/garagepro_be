using BusinessObject;
using Repositories;
using ColorEntity = BusinessObject.Color;

namespace Services
{
    public class ColorService : IColorService
    {
        private readonly IColorRepository _colorRepository;

        public ColorService(IColorRepository colorRepository)
        {
            _colorRepository = colorRepository;
        }

        public async Task<IEnumerable<ColorEntity>> GetActiveColorsAsync()
        {
            return await _colorRepository.GetActiveColorsAsync();
        }

        public async Task<bool> ColorExistsAsync(Guid id)
        {
            return await _colorRepository.ExistsAsync(id);
        }

        public async Task InitializeDefaultColorsAsync()
        {
            await _colorRepository.InitializeDefaultColorsAsync();
        }
    }
}