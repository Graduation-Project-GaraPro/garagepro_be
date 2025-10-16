using AutoMapper;
using BusinessObject.Quotations;
using Customers;
using Repositories.QuotationRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.QuotationService
{
    public class QuotationService : IQuotationService
    {
        private readonly IQuotationRepository _quotationRepository;
        private readonly IMapper _mapper;
        

        public QuotationService(IQuotationRepository quotationRepository, IMapper mapper)
        {
            _quotationRepository = quotationRepository;
            _mapper = mapper;
        }

        // ✅ Lấy tất cả báo giá theo userId (tất cả RepairRequest thuộc user đó)
        public async Task<IEnumerable<QuotationDto>> GetQuotationsByUserIdAsync(string userId)
        {
            var quotations = await _quotationRepository.GetQuotationsByUserIdAsync(userId);

            // Map tự động bằng AutoMapper
            var result = _mapper.Map<IEnumerable<QuotationDto>>(quotations);

            return result;
        }

        // ✅ Lấy danh sách báo giá thuộc một RepairRequest cụ thể
        public async Task<IEnumerable<QuotationDto>> GetQuotationsByRepairRequestIdAsync(string userId, Guid repairRequestId)
        {
            var quotations = await _quotationRepository.GetQuotationsByRepairRequestIdAsync(userId, repairRequestId);

            var result = _mapper.Map<IEnumerable<QuotationDto>>(quotations);

            return result;
        }

        public async Task<bool> ApproveQuotationAsync(Guid quotationId)
        {
            var quotation =await _quotationRepository.GetQuotationByIdAsync(quotationId);
            if(quotation == null)
            {
                throw new KeyNotFoundException($"Quotation with ID {quotationId} was not found.");
            }
            if(quotation.Status != Status.Pending)
            {
                throw new InvalidOperationException($"Quotation with ID {quotationId} has already been approved.");
            }
            quotation.Status = Status.Approved;
            quotation.ApprovedAt = DateTime.Now;
             await _quotationRepository.UpdateQuotationAsync(quotation);
            return true;
        }

        public async Task<bool> RejectQuotationAsync(Guid quotationId)
        {
            var quotation = await _quotationRepository.GetQuotationByIdAsync(quotationId);
            if (quotation == null)
            {
                throw new KeyNotFoundException($"Quotation with ID {quotationId} was not found.");
            }
            if (quotation.Status != Status.Pending)
            {
                throw new InvalidOperationException($"Quotation with ID {quotationId} has already been approved.");
            }
            quotation.Status = Status.Approved;//rệct
            quotation.ApprovedAt = DateTime.Now;
            await _quotationRepository.UpdateQuotationAsync(quotation);
            return true;
        }
    }
}
