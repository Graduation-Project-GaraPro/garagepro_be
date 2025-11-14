using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.PaymentRepositories
{
    public interface IPaymentRepository
    {
      
            Task<Payment> GetByIdAsync(long paymentId);
            Task<IEnumerable<Payment>> GetAllAsync();
            Task AddAsync(Payment payment);

            Task<Payment> GetByConditionAsync(Expression<Func<Payment, bool>> predicate);
            Task<Payment> GetByConditionAsync(Expression<Func<Payment, bool>> predicate, CancellationToken ct);

            Task SaveChangesAsync(CancellationToken ct);
            Task UpdateAsync(Payment payment ,CancellationToken ct);
            Task DeleteAsync(Guid paymentId);

            // Optional - Additional methods
            //Task<IEnumerable<Payment>> GetByUserIdAsync(string userId);
            //Task<IEnumerable<Payment>> GetByRepairOrderIdAsync(Guid repairOrderId);
            //Task<IEnumerable<Payment>> GetByStatusAsync(string status);
        }
    }