using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.PaymentRepositories
{
    public interface IPaymentRepository
    {
      
            Task<Payment> GetByIdAsync(Guid paymentId);
            Task<IEnumerable<Payment>> GetAllAsync();
            Task AddAsync(Payment payment);
            Task UpdateAsync(Payment payment);
            Task DeleteAsync(Guid paymentId);

            // Optional - Additional methods
            //Task<IEnumerable<Payment>> GetByUserIdAsync(string userId);
            //Task<IEnumerable<Payment>> GetByRepairOrderIdAsync(Guid repairOrderId);
            //Task<IEnumerable<Payment>> GetByStatusAsync(string status);
        }
    }