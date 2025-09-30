using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using ColorEntity = BusinessObject.Color;

namespace Repositories
{
    public class ColorRepository : IColorRepository
    {
        private readonly MyAppDbContext _context;

        public ColorRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ColorEntity>> GetActiveColorsAsync()
        {
            return await _context.Colors
                .Where(c => c.IsActive)
                .OrderBy(c => c.ColorName)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Colors.AnyAsync(c => c.ColorId == id);
        }

        public async Task InitializeDefaultColorsAsync()
        {
            var existingColors = await _context.Colors.ToListAsync();
            
            var defaultColors = new List<ColorEntity>
            {
                new ColorEntity { ColorId = Guid.NewGuid(), ColorName = "Red", HexCode = "#FF5733", IsActive = true },
                new ColorEntity { ColorId = Guid.NewGuid(), ColorName = "Blue", HexCode = "#3498DB", IsActive = true },
                new ColorEntity { ColorId = Guid.NewGuid(), ColorName = "Green", HexCode = "#28A745", IsActive = true },
                new ColorEntity { ColorId = Guid.NewGuid(), ColorName = "Yellow", HexCode = "#FFC107", IsActive = true },
                new ColorEntity { ColorId = Guid.NewGuid(), ColorName = "Orange", HexCode = "#FF8C00", IsActive = true },
                new ColorEntity { ColorId = Guid.NewGuid(), ColorName = "Purple", HexCode = "#8E44AD", IsActive = true },
                new ColorEntity { ColorId = Guid.NewGuid(), ColorName = "Pink", HexCode = "#E91E63", IsActive = true },
                new ColorEntity { ColorId = Guid.NewGuid(), ColorName = "Teal", HexCode = "#17A2B8", IsActive = true },
                new ColorEntity { ColorId = Guid.NewGuid(), ColorName = "Gray", HexCode = "#6C757D", IsActive = true },
                new ColorEntity { ColorId = Guid.NewGuid(), ColorName = "Dark Blue", HexCode = "#0D47A1", IsActive = true },
                new ColorEntity { ColorId = Guid.NewGuid(), ColorName = "Dark Green", HexCode = "#1B5E20", IsActive = true },
                new ColorEntity { ColorId = Guid.NewGuid(), ColorName = "Dark Red", HexCode = "#B71C1C", IsActive = true }
            };

            foreach (var defaultColor in defaultColors)
            {
                if (!existingColors.Any(c => c.HexCode == defaultColor.HexCode))
                {
                    _context.Colors.Add(defaultColor);
                }
            }
            
            await _context.SaveChangesAsync();
        }
    }
}