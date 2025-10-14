using AutoMapper;
using BusinessObject;
using Dtos.Quotations;
using Repositories.QuotationRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.QuotationServices
{
    public class QuotationManagementService : IQuotationService // Renamed class to avoid conflict
    {
        private readonly IQuotationRepository _quotationRepository;
        private readonly IQuotationServiceRepository _quotationServiceRepository;
        private readonly IQuotationPartRepository _quotationPartRepository;
        private readonly IMapper _mapper;

        public QuotationManagementService(
            IQuotationRepository quotationRepository,
            IQuotationServiceRepository quotationServiceRepository,
            IQuotationPartRepository quotationPartRepository,
            IMapper mapper)
        {
            _quotationRepository = quotationRepository;
            _quotationServiceRepository = quotationServiceRepository;
            _quotationPartRepository = quotationPartRepository;
            _mapper = mapper;
        }

        public async Task<QuotationDto> CreateQuotationAsync(CreateQuotationDto createQuotationDto)
        {
            // Create the main quotation
            var quotation = new Quotation
            {
                InspectionId = createQuotationDto.InspectionId,
                UserId = createQuotationDto.UserId,
                VehicleId = createQuotationDto.VehicleId,
                Note = createQuotationDto.Note,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            var createdQuotation = await _quotationRepository.CreateAsync(quotation);

            // Create quotation services
            if (createQuotationDto.QuotationServices != null)
            {
                foreach (var serviceDto in createQuotationDto.QuotationServices)
                {
                    var quotationService = new QuotationService // This is the entity
                    {
                        QuotationId = createdQuotation.QuotationId,
                        ServiceId = serviceDto.ServiceId,
                        IsSelected = serviceDto.IsSelected,
                        Price = serviceDto.Price,
                        Quantity = serviceDto.Quantity,
                        TotalPrice = serviceDto.Price * serviceDto.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    await _quotationServiceRepository.CreateAsync(quotationService);
                    createdQuotation.QuotationServices.Add(quotationService);
                }
            }

            // Create quotation parts
            if (createQuotationDto.QuotationParts != null)
            {
                foreach (var partDto in createQuotationDto.QuotationParts)
                {
                    var quotationPart = new QuotationPart
                    {
                        QuotationId = createdQuotation.QuotationId,
                        PartId = partDto.PartId,
                        IsSelected = partDto.IsSelected,
                        Price = partDto.Price,
                        Quantity = partDto.Quantity,
                        TotalPrice = partDto.Price * partDto.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    await _quotationPartRepository.CreateAsync(quotationPart);
                    createdQuotation.QuotationParts.Add(quotationPart);
                }
            }

            // Calculate total amount
            createdQuotation.TotalAmount = 
                createdQuotation.QuotationServices.Sum(qs => qs.TotalPrice) +
                createdQuotation.QuotationParts.Sum(qp => qp.TotalPrice);

            await _quotationRepository.UpdateAsync(createdQuotation);

            return _mapper.Map<QuotationDto>(createdQuotation);
        }

        public async Task<QuotationDto> GetQuotationByIdAsync(Guid quotationId)
        {
            var quotation = await _quotationRepository.GetByIdAsync(quotationId);
            return quotation == null ? null : _mapper.Map<QuotationDto>(quotation);
        }

        public async Task<IEnumerable<QuotationDto>> GetQuotationsByInspectionIdAsync(Guid inspectionId)
        {
            var quotations = await _quotationRepository.GetByInspectionIdAsync(inspectionId);
            return quotations.Select(q => _mapper.Map<QuotationDto>(q));
        }

        public async Task<IEnumerable<QuotationDto>> GetQuotationsByUserIdAsync(string userId)
        {
            var quotations = await _quotationRepository.GetByUserIdAsync(userId);
            return quotations.Select(q => _mapper.Map<QuotationDto>(q));
        }

        public async Task<IEnumerable<QuotationDto>> GetAllQuotationsAsync()
        {
            var quotations = await _quotationRepository.GetAllAsync();
            return quotations.Select(q => _mapper.Map<QuotationDto>(q));
        }

        public async Task<QuotationDto> UpdateQuotationAsync(Guid quotationId, UpdateQuotationDto updateQuotationDto)
        {
            var existingQuotation = await _quotationRepository.GetByIdAsync(quotationId);
            if (existingQuotation == null)
                throw new ArgumentException($"Quotation with ID {quotationId} not found.");

            _mapper.Map(updateQuotationDto, existingQuotation);
            // Note: Quotation entity doesn't have UpdatedAt property, so we skip it

            var updatedQuotation = await _quotationRepository.UpdateAsync(existingQuotation);
            return _mapper.Map<QuotationDto>(updatedQuotation);
        }

        public async Task<QuotationDto> UpdateQuotationStatusAsync(Guid quotationId, UpdateQuotationStatusDto updateStatusDto)
        {
            var existingQuotation = await _quotationRepository.GetByIdAsync(quotationId);
            if (existingQuotation == null)
                throw new ArgumentException($"Quotation with ID {quotationId} not found.");

            existingQuotation.Status = updateStatusDto.Status;
            
            if (updateStatusDto.CustomerResponseAt.HasValue)
            {
                existingQuotation.CustomerResponseAt = updateStatusDto.CustomerResponseAt.Value;
            }
            
            if (updateStatusDto.Status == "Sent")
            {
                existingQuotation.SentToCustomerAt = DateTime.UtcNow;
            }

            var updatedQuotation = await _quotationRepository.UpdateAsync(existingQuotation);
            return _mapper.Map<QuotationDto>(updatedQuotation);
        }

        public async Task<bool> DeleteQuotationAsync(Guid quotationId)
        {
            return await _quotationRepository.DeleteAsync(quotationId);
        }

        public async Task<bool> QuotationExistsAsync(Guid quotationId)
        {
            return await _quotationRepository.ExistsAsync(quotationId);
        }

        public async Task<QuotationDto> ProcessCustomerResponseAsync(CustomerQuotationResponseDto responseDto)
        {
            var quotation = await _quotationRepository.GetByIdAsync(responseDto.QuotationId);
            if (quotation == null)
                throw new ArgumentException($"Quotation with ID {responseDto.QuotationId} not found.");

            // Update quotation status
            quotation.Status = responseDto.Status;
            quotation.CustomerResponseAt = DateTime.UtcNow;
            
            // Update selected services
            if (responseDto.SelectedServices != null)
            {
                foreach (var selectedService in responseDto.SelectedServices)
                {
                    var quotationService = quotation.QuotationServices
                        .FirstOrDefault(qs => qs.QuotationServiceId == selectedService.QuotationServiceId);
                    if (quotationService != null)
                    {
                        quotationService.IsSelected = true;
                    }
                }
            }

            // Update selected parts
            if (responseDto.SelectedParts != null)
            {
                foreach (var selectedPart in responseDto.SelectedParts)
                {
                    var quotationPart = quotation.QuotationParts
                        .FirstOrDefault(qp => qp.QuotationPartId == selectedPart.QuotationPartId);
                    if (quotationPart != null)
                    {
                        quotationPart.IsSelected = true;
                    }
                }
            }

            // Recalculate total amount based on selected items
            quotation.TotalAmount = 
                quotation.QuotationServices.Where(qs => qs.IsSelected).Sum(qs => qs.TotalPrice) +
                quotation.QuotationParts.Where(qp => qp.IsSelected).Sum(qp => qp.TotalPrice);

            var updatedQuotation = await _quotationRepository.UpdateAsync(quotation);
            return _mapper.Map<QuotationDto>(updatedQuotation);
        }
    }
}