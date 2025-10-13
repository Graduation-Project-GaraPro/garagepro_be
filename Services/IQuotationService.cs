using BusinessObject.Enums;
using Dtos.Quotation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
    public interface IQuotationService
    {
        Task<QuotationDto> GetQuotationByIdAsync(Guid id);
        Task<QuotationDto> GetQuotationByInspectionIdAsync(Guid inspectionId);
        Task<IEnumerable<QuotationDto>> GetAllQuotationsAsync();
        Task<IEnumerable<QuotationDto>> GetQuotationsByUserIdAsync(string userId);
        Task<IEnumerable<QuotationDto>> GetQuotationsByStatusAsync(QuotationStatus status);
        Task<QuotationDto> CreateQuotationAsync(Guid inspectionId, string userId);
        Task<QuotationDto> UpdateQuotationAsync(Guid id, QuotationDto quotationDto);
        Task<QuotationDto> SendQuotationToCustomerAsync(Guid id);
        Task<QuotationDto> ApproveQuotationAsync(Guid id);
        Task<QuotationDto> RejectQuotationAsync(Guid id, string rejectionReason);
        Task<QuotationDto> ReviseQuotationAsync(Guid id, string revisionDetails);
        Task<bool> DeleteQuotationAsync(Guid id);
    }
}