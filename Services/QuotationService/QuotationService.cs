using Customers;
using Repositories.QuotationRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.QuotationService
{
   
        public class QuotationService : IQuotationService
        {
            private readonly IQuotationRepository _quotationRepository;

            public QuotationService(IQuotationRepository quotationRepository)
            {
                _quotationRepository = quotationRepository;
            }

            // Lấy tất cả báo giá của customer
            public async Task<List<QuotationDto>> GetQuotationsByUserIdAsync(string userId)
            {
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentException("UserId cannot be null or empty");

                var quotations = await _quotationRepository.GetQuotationsByUserIdAsync(userId);
                return quotations;
            }

            // Lấy báo giá theo RepairRequest
            public async Task<List<QuotationDto>> GetQuotationsByRepairRequestIdAsync(string userId, Guid repairRequestId)
            {
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentException("UserId cannot be null or empty");

                if (repairRequestId == Guid.Empty)
                    throw new ArgumentException("RepairRequestId is invalid");

                var quotations = await _quotationRepository.GetQuotationsByRepairRequestIdAsync(userId, repairRequestId);
                return quotations;
            }

            // UpdateQuotationPartsAsync 
            // public async Task<QuotationDto> UpdateQuotationPartsAsync(string userId, UpdateQuotationPartsDto dto)
            // {
            //     if (dto == null) throw new ArgumentNullException(nameof(dto));
            //     var updatedQuotation = await _quotationRepository.UpdateQuotationPartsAsync(userId, dto);
            //     return updatedQuotation;
            // }
        }
    }


