using AutoMapper;
using BusinessObject;
using BusinessObject.Enums; // Add this using statement
using Dtos.Quotations;
using Repositories.QuotationRepositories;
using Repositories.ServiceRepositories;
using Repositories.PartRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositories; // Add this using statement

namespace Services.QuotationServices
{
    public class QuotationManagementService : IQuotationService
    {
        private readonly IQuotationRepository _quotationRepository;
        private readonly IQuotationServiceRepository _quotationServiceRepository;
        private readonly IQuotationServicePartRepository _quotationServicePartRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IPartRepository _partRepository;
        private readonly IRepairOrderRepository _repairOrderRepository; 
        private readonly IMapper _mapper;

        public QuotationManagementService(
            IQuotationRepository quotationRepository,
            IQuotationServiceRepository quotationServiceRepository,
            IQuotationServicePartRepository quotationServicePartRepository, 
            IMapper mapper) : this(quotationRepository, quotationServiceRepository, quotationServicePartRepository, null, null, null, mapper)
        {
        }

        public QuotationManagementService(
            IQuotationRepository quotationRepository,
            IQuotationServiceRepository quotationServiceRepository,
            IQuotationServicePartRepository quotationServicePartRepository,
            IServiceRepository serviceRepository,
            IPartRepository partRepository,
            IRepairOrderRepository repairOrderRepository,
            IMapper mapper)
        {
            _quotationRepository = quotationRepository;
            _quotationServiceRepository = quotationServiceRepository;
            _quotationServicePartRepository = quotationServicePartRepository;
            _serviceRepository = serviceRepository;
            _partRepository = partRepository;
            _repairOrderRepository = repairOrderRepository;
            _mapper = mapper;
        }

        public async Task<QuotationDto> CreateQuotationAsync(CreateQuotationDto createQuotationDto)
        {
            // If RepairOrderId is provided, get UserId and VehicleId from the RepairOrder
            string userId = createQuotationDto.UserId;
            Guid vehicleId = createQuotationDto.VehicleId;
            
            if (createQuotationDto.RepairOrderId.HasValue)
            {
                var repairOrder = await _repairOrderRepository.GetByIdAsync(createQuotationDto.RepairOrderId.Value);
                if (repairOrder != null)
                {
                    userId = repairOrder.UserId;
                    vehicleId = repairOrder.VehicleId;
                }
            }

            // Create the main quotation
            var quotation = new Quotation
            {
                // Made InspectionId nullable to allow creating quotes without inspection
                InspectionId = createQuotationDto.InspectionId ?? default(Guid?), 
                RepairOrderId = createQuotationDto.RepairOrderId ?? default(Guid?), // Add RepairOrderId
                UserId = userId,
                VehicleId = vehicleId,
                Note = createQuotationDto.Note,
                Status = QuotationStatus.Pending, // Use enum instead of string
                CreatedAt = DateTime.UtcNow,
                TotalAmount = 0, // Will be calculated
                DiscountAmount = 0
            };

            var createdQuotation = await _quotationRepository.CreateAsync(quotation);

            // Create quotation services and calculate total
            decimal totalAmount = 0;
            if (createQuotationDto.QuotationServices != null)
            {
                foreach (var serviceDto in createQuotationDto.QuotationServices)
                {
                    // Get the actual service to retrieve its price
                    var service = await _serviceRepository.GetByIdAsync(serviceDto.ServiceId);
                    if (service == null)
                    {
                        throw new ArgumentException($"Service with ID {serviceDto.ServiceId} not found.");
                    }

                    // Calculate service total (price * quantity) - using default quantity of 1
                    decimal serviceTotal = service.Price * 1;
                    totalAmount += serviceTotal;

                    var quotationService = new QuotationService // This is the entity
                    {
                        QuotationId = createdQuotation.QuotationId,
                        ServiceId = serviceDto.ServiceId,
                        IsSelected = serviceDto.IsSelected,
                        Price = service.Price // Store the actual service price at the time of quotation creation
                    };
                    
                    await _quotationServiceRepository.CreateAsync(quotationService);
                    // Don't add to the collection directly, let the repository handle the relationship
                    
                    // Create quotation service parts for this service
                    if (serviceDto.QuotationServiceParts != null)
                    {
                        foreach (var partDto in serviceDto.QuotationServiceParts)
                        {
                            // Get the actual part to retrieve its price
                            var part = await _partRepository.GetByIdAsync(partDto.PartId);
                            if (part == null)
                            {
                                throw new ArgumentException($"Part with ID {partDto.PartId} not found.");
                            }

                            // Calculate part total (price * quantity) - using default quantity of 1
                            decimal partTotal = part.Price * 1;
                            totalAmount += partTotal;

                            var quotationServicePart = new QuotationServicePart
                            {
                                QuotationServiceId = quotationService.QuotationServiceId,
                                PartId = partDto.PartId,
                                IsSelected = partDto.IsSelected,
                                // Set the new properties
                                IsRecommended = partDto.IsRecommended,
                                Price = part.Price, // Store the actual part price at the time of quotation creation
                                Quantity = partDto.Quantity
                            };
                            
                            await _quotationServicePartRepository.CreateAsync(quotationServicePart);
                            // Don't add to the collection directly, let the repository handle the relationship
                        }
                    }
                }
            }

            // Update the quotation with the calculated total amount
            createdQuotation.TotalAmount = totalAmount;
            await _quotationRepository.UpdateAsync(createdQuotation);

            // Reload the quotation with all related data to ensure we have the complete object
            var completeQuotation = await _quotationRepository.GetByIdAsync(createdQuotation.QuotationId);
            return _mapper.Map<QuotationDto>(completeQuotation);
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

        public async Task<IEnumerable<QuotationDto>> GetQuotationsByRepairOrderIdAsync(Guid repairOrderId)
        {
            var quotations = await _quotationRepository.GetByRepairOrderIdAsync(repairOrderId);
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

            // Note: Removed recalculation of TotalAmount as it's no longer in the Quotation entity

            var updatedQuotation = await _quotationRepository.UpdateAsync(quotation);
            return _mapper.Map<QuotationDto>(updatedQuotation);
        }
    }
}