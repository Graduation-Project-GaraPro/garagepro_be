using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

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

    public class LabelRepository : ILabelRepository
    {
        private readonly MyAppDbContext _context;

        public LabelRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Label>> GetAllAsync()
        {
            return await _context.Labels
                .Include(l => l.OrderStatus)
                .ToListAsync();
        }

        public async Task<IEnumerable<Label>> GetByOrderStatusIdAsync(Guid orderStatusId)
        {
            return await _context.Labels
                .Include(l => l.OrderStatus)
                .Where(l => l.OrderStatusId == orderStatusId)
                .ToListAsync();
        }

        public async Task<Label?> GetByIdAsync(Guid id)
        {
            return await _context.Labels
                .Include(l => l.OrderStatus)
                .FirstOrDefaultAsync(l => l.LabelId == id);
        }

        public async Task<Label> CreateAsync(Label label)
        {
            _context.Labels.Add(label);
            await _context.SaveChangesAsync();
            return label;
        }

        public async Task<Label> UpdateAsync(Label label)
        {
            _context.Labels.Update(label);
            await _context.SaveChangesAsync();
            return label;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var label = await _context.Labels.FindAsync(id);
            if (label == null) return false;

            _context.Labels.Remove(label);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Labels.AnyAsync(l => l.LabelId == id);
        }
    }
}