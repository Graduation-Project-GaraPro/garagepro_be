using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using Dtos.Quotation;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class QuotationService : IQuotationService
    {
        private readonly IQuotationRepository _quotationRepository;
        private readonly IMapper _mapper;
        private readonly MyAppDbContext _context;

        public QuotationService(IQuotationRepository quotationRepository, IMapper mapper, MyAppDbContext context)
        {
            _quotationRepository = quotationRepository;
            _mapper = mapper;
            _context = context;
        }

        public async Task<QuotationDto> GetQuotationByIdAsync(Guid id)
        {
            var quotation = await _quotationRepository.GetByIdAsync(id);
            return _mapper.Map<QuotationDto>(quotation);
        }

        public async Task<QuotationDto> GetQuotationByInspectionIdAsync(Guid inspectionId)
        {
            var quotation = await _quotationRepository.GetByInspectionIdAsync(inspectionId);
            return _mapper.Map<QuotationDto>(quotation);
        }

        public async Task<IEnumerable<QuotationDto>> GetAllQuotationsAsync()
        {
            var quotations = await _quotationRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<QuotationDto>>(quotations);
        }

        public async Task<IEnumerable<QuotationDto>> GetQuotationsByUserIdAsync(string userId)
        {
            var quotations = await _quotationRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<QuotationDto>>(quotations);
        }

        public async Task<IEnumerable<QuotationDto>> GetQuotationsByStatusAsync(QuotationStatus status)
        {
            var quotations = await _quotationRepository.GetByStatusAsync(status);
            return _mapper.Map<IEnumerable<QuotationDto>>(quotations);
        }

        public async Task<QuotationDto> CreateQuotationAsync(Guid inspectionId, string userId)
        {
            // Check if inspection exists
            var inspectionExists = await _context.Inspections.AnyAsync(i => i.InspectionId == inspectionId);
            if (!inspectionExists)
                throw new ArgumentException("Inspection not found");

            // Create a new quotation from inspection
            var quotation = new Quotation
            {
                InspectionId = inspectionId,
                UserId = userId,
                Status = QuotationStatus.Draft,
                CustomerNote = "",
                ChangeRequestDetails = ""
            };

            var createdQuotation = await _quotationRepository.CreateAsync(quotation);
            return _mapper.Map<QuotationDto>(createdQuotation);
        }

        public async Task<QuotationDto> UpdateQuotationAsync(Guid id, QuotationDto quotationDto)
        {
            var quotation = await _quotationRepository.GetByIdAsync(id);
            if (quotation == null)
                throw new ArgumentException("Quotation not found");

            // Update quotation properties
            quotation.CustomerNote = quotationDto.CustomerNote;
            quotation.ChangeRequestDetails = quotationDto.ChangeRequestDetails;
            quotation.EstimateExpiresAt = quotationDto.EstimateExpiresAt;
            quotation.TotalAmount = quotationDto.TotalAmount;

            var updatedQuotation = await _quotationRepository.UpdateAsync(quotation);
            return _mapper.Map<QuotationDto>(updatedQuotation);
        }

        public async Task<QuotationDto> SendQuotationToCustomerAsync(Guid id)
        {
            var quotation = await _quotationRepository.GetByIdAsync(id);
            if (quotation == null)
                throw new ArgumentException("Quotation not found");

            quotation.Status = QuotationStatus.Sent;
            quotation.SentAt = DateTime.UtcNow;

            var updatedQuotation = await _quotationRepository.UpdateAsync(quotation);
            return _mapper.Map<QuotationDto>(updatedQuotation);
        }

        public async Task<QuotationDto> ApproveQuotationAsync(Guid id)
        {
            var quotation = await _quotationRepository.GetByIdAsync(id);
            if (quotation == null)
                throw new ArgumentException("Quotation not found");

            quotation.Status = QuotationStatus.Approved;
            quotation.ResponseAt = DateTime.UtcNow;

            var updatedQuotation = await _quotationRepository.UpdateAsync(quotation);
            return _mapper.Map<QuotationDto>(updatedQuotation);
        }

        public async Task<QuotationDto> RejectQuotationAsync(Guid id, string rejectionReason)
        {
            var quotation = await _quotationRepository.GetByIdAsync(id);
            if (quotation == null)
                throw new ArgumentException("Quotation not found");

            quotation.Status = QuotationStatus.Rejected;
            quotation.ResponseAt = DateTime.UtcNow;
            quotation.ChangeRequestDetails = rejectionReason;

            var updatedQuotation = await _quotationRepository.UpdateAsync(quotation);
            return _mapper.Map<QuotationDto>(updatedQuotation);
        }

        public async Task<QuotationDto> ReviseQuotationAsync(Guid id, string revisionDetails)
        {
            var quotation = await _quotationRepository.GetByIdAsync(id);
            if (quotation == null)
                throw new ArgumentException("Quotation not found");

            // Create a new revision of the quotation
            var revisedQuotation = new Quotation
            {
                InspectionId = quotation.InspectionId,
                UserId = quotation.UserId,
                Status = QuotationStatus.Draft,
                CustomerNote = quotation.CustomerNote,
                ChangeRequestDetails = revisionDetails,
                EstimateExpiresAt = quotation.EstimateExpiresAt,
                OriginalQuotationId = quotation.QuotationId,
                RevisionNumber = quotation.RevisionNumber + 1
            };

            var createdQuotation = await _quotationRepository.CreateAsync(revisedQuotation);
            return _mapper.Map<QuotationDto>(createdQuotation);
        }

        public async Task<bool> DeleteQuotationAsync(Guid id)
        {
            return await _quotationRepository.DeleteAsync(id);
        }
    }
}