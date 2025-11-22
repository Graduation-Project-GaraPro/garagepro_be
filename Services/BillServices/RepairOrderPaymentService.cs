using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dtos.Bills;
using Repositories;

namespace Services.BillServices
{
    public class RepairOrderPaymentService : IRepairOrderPaymentService
    {
        private readonly IRepairOrderRepository _repairOrderRepository;
        private readonly IMapper _mapper;

        public RepairOrderPaymentService(
            IRepairOrderRepository repairOrderRepository,
            IMapper mapper)
        {
            _repairOrderRepository = repairOrderRepository;
            _mapper = mapper;
        }

        public async Task<RepairOrderPaymentDto?> GetRepairOrderPaymentInfoAsync(Guid repairOrderId, string userId)
        {
            var repairOrder = await _repairOrderRepository.GetRepairOrderForPaymentAsync(repairOrderId, userId);

            if (repairOrder == null)
                return null;

            // Nếu bạn không dùng filtered Include, có thể filter ở đây:
            // repairOrder.Quotations = repairOrder.Quotations
            //     .Where(q => q.Status == QuotationStatus.Approved)
            //     .ToList();

            var dto = _mapper.Map<RepairOrderPaymentDto>(repairOrder);
            return dto;
        }
    }
}
