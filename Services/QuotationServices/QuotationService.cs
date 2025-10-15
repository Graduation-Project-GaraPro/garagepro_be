using AutoMapper;
using BusinessObject;
using BusinessObject.Enums; // Add this using statement
using Dtos.Quotations;
using Repositories.QuotationRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.QuotationServices
{
    public class QuotationManagementService : IQuotationService
    {
        private readonly IQuotationRepository _quotationRepository;
        private readonly IQuotationServiceRepository _quotationServiceRepository;
        private readonly IQuotationServicePartRepository _quotationServicePartRepository; // Changed from IQuotationPartRepository
        private readonly IMapper _mapper;

        public QuotationManagementService(
            IQuotationRepository quotationRepository,
            IQuotationServiceRepository quotationServiceRepository,
            IQuotationServicePartRepository quotationServicePartRepository, // Changed from IQuotationPartRepository
            IMapper mapper)
        {
            _quotationRepository = quotationRepository;
            _quotationServiceRepository = quotationServiceRepository;
            _quotationServicePartRepository = quotationServicePartRepository; // Changed from _quotationPartRepository
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
                Status = QuotationStatus.Pending, // Use enum instead of string
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
                    
                    // Create quotation service parts for this service
                    if (serviceDto.QuotationServiceParts != null)
                    {
                        foreach (var partDto in serviceDto.QuotationServiceParts)
                        {
                            var quotationServicePart = new QuotationServicePart
                            {
                                QuotationServiceId = quotationService.QuotationServiceId,
                                PartId = partDto.PartId,
                                IsSelected = partDto.IsSelected,
                                // Set the new properties
                                IsRecommended = partDto.IsRecommended,
                                RecommendationNote = partDto.RecommendationNote,
                                Price = partDto.Price,
                                Quantity = partDto.Quantity,
                                TotalPrice = partDto.Price * partDto.Quantity,
                                CreatedAt = DateTime.UtcNow
                            };
                            
                            await _quotationServicePartRepository.CreateAsync(quotationServicePart);
                            quotationService.QuotationServiceParts.Add(quotationServicePart);
                        }
                    }
                }
            }

            // Calculate total amount (services + parts)
            createdQuotation.TotalAmount = 
                createdQuotation.QuotationServices.Sum(qs => qs.TotalPrice) +
                createdQuotation.QuotationServices.Sum(qs => qs.QuotationServiceParts.Sum(qsp => qsp.TotalPrice));

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

            // Convert string status to enum
            existingQuotation.Status = Enum.Parse<QuotationStatus>(updateStatusDto.Status);
            
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
            // Get the quotation with all related data
            var quotation = await _quotationRepository.GetByIdAsync(responseDto.QuotationId);
            if (quotation == null)
                throw new ArgumentException($"Quotation with ID {responseDto.QuotationId} not found.");

            // Update quotation status
            quotation.Status = Enum.Parse<QuotationStatus>(responseDto.Status);
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

            // Update selected service parts
            if (responseDto.SelectedServiceParts != null)
            {
                foreach (var selectedServicePart in responseDto.SelectedServiceParts)
                {
                    // Find the service part in all quotation services
                    var quotationServicePart = quotation.QuotationServices
                        .SelectMany(qs => qs.QuotationServiceParts)
                        .FirstOrDefault(qsp => qsp.QuotationServicePartId == selectedServicePart.QuotationServicePartId);
                        
                    if (quotationServicePart != null)
                    {
                        quotationServicePart.IsSelected = true;
                    }
                }
            }

            // Recalculate total amount based on selected services and parts
            quotation.TotalAmount = 
                quotation.QuotationServices.Where(qs => qs.IsSelected).Sum(qs => qs.TotalPrice) +
                quotation.QuotationServices
                    .Where(qs => qs.IsSelected)
                    .SelectMany(qs => qs.QuotationServiceParts)
                    .Where(qsp => qsp.IsSelected)
                    .Sum(qsp => qsp.TotalPrice);

            var updatedQuotation = await _quotationRepository.UpdateAsync(quotation);
            return _mapper.Map<QuotationDto>(updatedQuotation);
        }
    }
}