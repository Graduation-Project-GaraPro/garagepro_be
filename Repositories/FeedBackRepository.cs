using BusinessObject.Manager;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
    public class FeedBackRepository : IFeedBackRepository
    {
        private readonly MyAppDbContext _context;

        public FeedBackRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(FeedBack feedback)
        {
            _context.FeedBacks.Add(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid feedbackId)
        {
            var entity = await _context.FeedBacks.FindAsync(feedbackId);
            if (entity != null)
            {
                _context.FeedBacks.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<FeedBack>> GetAllAsync()
        {
            return await _context.FeedBacks
                                 .Include(f => f.User)          
                                 .Include(f => f.RepairOrder)   
                                 .ToListAsync();
        }

        public async Task<FeedBack> GetByIdAsync(Guid feedbackId)
        {
            return await _context.FeedBacks
                                 .Include(f => f.User)
                                 .Include(f => f.RepairOrder)
                                 .FirstOrDefaultAsync(f => f.FeedBackId == feedbackId);
        }

        public async Task<List<FeedBack>> GetByRepairOrderIdAsync(Guid repairOrderId)
        {
            return await _context.FeedBacks
                                 .Where(f => f.RepairOrderId == repairOrderId)
                                 .ToListAsync();
        }

        public async Task<List<FeedBack>> GetByUserIdAsync(string userId)
        {
            return await _context.FeedBacks
                                 .Where(f => f.UserId == userId)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<FeedBack>> GetFeedbackByBranchIdAsync(Guid BranchId)
        {
            return await Task.FromResult(_context.FeedBacks
                                 .Include(f => f.RepairOrder)
                                 .Where(f => f.RepairOrder.BranchId == BranchId)
                                 .AsEnumerable());
        }

        public async Task UpdateAsync(FeedBack feedback)
        {
            _context.FeedBacks.Update(feedback);
            await _context.SaveChangesAsync();
        }
    }
}
