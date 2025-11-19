using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dtos.Bills;

namespace Services.BillServices
{
    public interface IRepairOrderPaymentService
    {
        Task<RepairOrderPaymentDto?> GetRepairOrderPaymentInfoAsync(Guid repairOrderId, string userId);
    }
}
