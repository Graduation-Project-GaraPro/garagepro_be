using BusinessObject.Enums;
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

        Task<QuotationDetailDto> GetQuotationDetailByIdAsync(Guid quotationId);

        Task<object> GetQuotationsByUserIdAsync(
            string userId,
            int pageNumber,
            int pageSize,
            QuotationStatus? status);
        Task<QuotationDto> UpdateQuotationAsync(Guid quotationId, UpdateQuotationDto updateQuotationDto);
        Task<QuotationDto> UpdateQuotationStatusAsync(Guid quotationId, UpdateQuotationStatusDto updateStatusDto);
        Task<QuotationDetailDto> UpdateQuotationDetailsAsync(Guid quotationId, UpdateQuotationDetailsDto updateDto);
        Task<bool> DeleteQuotationAsync(Guid quotationId);
        Task<bool> QuotationExistsAsync(Guid quotationId);
        // ProcessCustomerResponseAsync removed - use ICustomerResponseQuotationService instead
        Task<bool> ApproveQuotationAsync(Guid quotationId);
        Task<bool> RejectQuotationAsync(Guid quotationId);
        // New method to copy quotation to jobs
        Task<bool> CopyQuotationToJobsAsync(Guid quotationId);
        // New method to create revision jobs
        Task<bool> CreateRevisionJobsAsync(Guid quotationId, string revisionReason);
    }
}