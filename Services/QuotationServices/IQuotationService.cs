using Dtos.Quotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.QuotationServices
{
    public interface IQuotationService // This can stay the same since it's an interface
    {
        Task<QuotationDto> CreateQuotationAsync(CreateQuotationDto createQuotationDto);
        Task<QuotationDto> GetQuotationByIdAsync(Guid quotationId);
        Task<IEnumerable<QuotationDto>> GetQuotationsByInspectionIdAsync(Guid inspectionId);
        Task<IEnumerable<QuotationDto>> GetQuotationsByUserIdAsync(string userId);
        Task<IEnumerable<QuotationDto>> GetQuotationsByRepairOrderIdAsync(Guid repairOrderId);
        Task<IEnumerable<QuotationDto>> GetAllQuotationsAsync();
        Task<QuotationDto> UpdateQuotationAsync(Guid quotationId, UpdateQuotationDto updateQuotationDto);
        Task<QuotationDto> UpdateQuotationStatusAsync(Guid quotationId, UpdateQuotationStatusDto updateStatusDto);
        Task<bool> DeleteQuotationAsync(Guid quotationId);
        Task<bool> QuotationExistsAsync(Guid quotationId);
        Task<QuotationDto> ProcessCustomerResponseAsync(CustomerQuotationResponseDto responseDto);
    }
}