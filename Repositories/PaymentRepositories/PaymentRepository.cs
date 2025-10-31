using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.PaymentRepositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly MyAppDbContext _context;

        public PaymentRepository(MyAppDbContext context)
        {
            _context = context;
        }

      
        public async Task<Payment> GetByIdAsync(Guid paymentId)
        {
            return await _context.Payments
                .Include(p => p.RepairOrder)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }

      
        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.RepairOrder)
                .Include(p => p.User)
                .ToListAsync();
        }

        
        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

       
        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        
        public async Task DeleteAsync(Guid paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Thêm method lấy payments theo user
        //public async Task<IEnumerable<Payment>> GetByUserIdAsync(string userId)
        //{
        //    return await _context.Payments
        //        .Include(p => p.RepairOrder)
        //        .Include(p => p.User)
        //        .Where(p => p.UserId == userId)
        //        .OrderByDescending(p => p.CreatedAt)
        //        .ToListAsync();
        //}

        //// ✅ Thêm method lấy payments theo repair order
        //public async Task<IEnumerable<Payment>> GetByRepairOrderIdAsync(Guid repairOrderId)
        //{
        //    return await _context.Payments
        //        .Include(p => p.RepairOrder)
        //        .Include(p => p.User)
        //        .Where(p => p.RepairOrderId == repairOrderId)
        //        .OrderByDescending(p => p.CreatedAt)
        //        .ToListAsync();
        //}

        //// ✅ Thêm method lấy payments theo payment status
        //public async Task<IEnumerable<Payment>> GetByStatusAsync(string status)
        //{
        //    return await _context.Payments
        //        .Include(p => p.RepairOrder)
        //        .Include(p => p.User)
        //        .Where(p => p.Status == status)
        //        .OrderByDescending(p => p.CreatedAt)
        //        .ToListAsync();
        //}
    }
}