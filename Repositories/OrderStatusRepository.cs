using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class OrderStatusRepository : IOrderStatusRepository
    {
        private readonly MyAppDbContext _context;

        public OrderStatusRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderStatus>> GetAllAsync()
        {
            return await _context.OrderStatuses
                .Include(os => os.Labels)
                .Include(os => os.RepairOrders)
                .ToListAsync();
        }

        public async Task<OrderStatus?> GetByIdAsync(Guid id)
        {
            return await _context.OrderStatuses
                .Include(os => os.Labels)
                .Include(os => os.RepairOrders)
                .FirstOrDefaultAsync(os => os.OrderStatusId == id);
        }

        public async Task<OrderStatus> CreateAsync(OrderStatus orderStatus)
        {
            _context.OrderStatuses.Add(orderStatus);
            await _context.SaveChangesAsync();
            return orderStatus;
        }

        public async Task<OrderStatus> UpdateAsync(OrderStatus orderStatus)
        {
            _context.OrderStatuses.Update(orderStatus);
            await _context.SaveChangesAsync();
            return orderStatus;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var orderStatus = await _context.OrderStatuses.FindAsync(id);
            if (orderStatus == null) return false;

            _context.OrderStatuses.Remove(orderStatus);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.OrderStatuses.AnyAsync(os => os.OrderStatusId == id);
        }

        public async Task<IEnumerable<Label>> GetLabelsByStatusIdAsync(Guid statusId)
        {
            return await _context.Labels
                .Where(l => l.OrderStatusId == statusId)
                .ToListAsync();
        }
    }
}