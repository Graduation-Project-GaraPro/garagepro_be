﻿using AutoMapper;
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
using Services; // Add this using statement

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
        private readonly IJobService _jobService; // Add this field

        public QuotationManagementService(
            IQuotationRepository quotationRepository,
            IQuotationServiceRepository quotationServiceRepository,
            IQuotationServicePartRepository quotationServicePartRepository, 
            IMapper mapper) : this(quotationRepository, quotationServiceRepository, quotationServicePartRepository, null, null, null, null, mapper)
        {
        }

        public QuotationManagementService(
            IQuotationRepository quotationRepository,
            IQuotationServiceRepository quotationServiceRepository,
            IQuotationServicePartRepository quotationServicePartRepository,
            IServiceRepository serviceRepository,
            IPartRepository partRepository,
            IRepairOrderRepository repairOrderRepository,
            IJobService jobService, // Add this parameter
            IMapper mapper)
        {
            _quotationRepository = quotationRepository;
            _quotationServiceRepository = quotationServiceRepository;
            _quotationServicePartRepository = quotationServicePartRepository;
            _serviceRepository = serviceRepository;
            _partRepository = partRepository;
            _repairOrderRepository = repairOrderRepository;
            _jobService = jobService; // Add this assignment
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
                        IsRequired = serviceDto.IsRequired, // Set the IsRequired flag
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
                                IsSelected = partDto.IsSelected, // Parts are automatically selected when service is selected
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
            try
            {
                var quotations = await _quotationRepository.GetByRepairOrderIdAsync(repairOrderId);
                return quotations.Select(q => _mapper.Map<QuotationDto>(q));
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error in GetQuotationsByRepairOrderIdAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Failed to retrieve quotations for repair order {repairOrderId}: {ex.Message}", ex);
            }
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
            
            // Handle service selections based on required/optional status
            // First, process all services to ensure required services stay selected
            foreach (var quotationService in quotation.QuotationServices)
            {
                // Required services must always be selected
                if (quotationService.IsRequired)
                {
                    quotationService.IsSelected = true;
                    
                    // Automatically select all parts for required services
                    foreach (var part in quotationService.QuotationServiceParts)
                    {
                        part.IsSelected = true;
                    }
                }
                else
                {
                    // For optional services, check if customer selected them
                    bool customerSelected = responseDto.SelectedServices != null && 
                        responseDto.SelectedServices.Any(s => s.QuotationServiceId == quotationService.QuotationServiceId);
                    
                    quotationService.IsSelected = customerSelected;
                    
                    // Select/deselect parts based on service selection
                    foreach (var part in quotationService.QuotationServiceParts)
                    {
                        part.IsSelected = customerSelected;
                    }
                }
            }

            var updatedQuotation = await _quotationRepository.UpdateAsync(quotation);
            return _mapper.Map<QuotationDto>(updatedQuotation);
        }

        public async Task<bool> ApproveQuotationAsync(Guid quotationId)
        {
            var quotation = await _quotationRepository.GetByIdAsync(quotationId);
            if (quotation == null)
                throw new ArgumentException($"Quotation with ID {quotationId} not found.");

            quotation.Status = BusinessObject.Enums.QuotationStatus.Approved;
            quotation.CustomerResponseAt = DateTime.UtcNow;

            // When approving, ensure required services are selected and select all parts for selected services
            foreach (var quotationService in quotation.QuotationServices)
            {
                // Ensure required services are selected
                if (quotationService.IsRequired)
                {
                    quotationService.IsSelected = true;
                }
                
                // Select all parts for selected services
                if (quotationService.IsSelected)
                {
                    foreach (var part in quotationService.QuotationServiceParts)
                    {
                        part.IsSelected = true;
                    }
                }
            }

            await _quotationRepository.UpdateAsync(quotation);
            
            // Note: We no longer auto-generate jobs here as per new requirements
            // Jobs will be manually created by manager using CopyQuotationToJobsAsync
            
            return true;
        }

        public async Task<bool> RejectQuotationAsync(Guid quotationId)
        {
            var quotation = await _quotationRepository.GetByIdAsync(quotationId);
            if (quotation == null)
                throw new ArgumentException($"Quotation with ID {quotationId} not found.");

            quotation.Status = BusinessObject.Enums.QuotationStatus.Rejected;
            quotation.CustomerResponseAt = DateTime.UtcNow;

            // When rejecting, deselect all services and their parts
            foreach (var quotationService in quotation.QuotationServices)
            {
                quotationService.IsSelected = false;
                // Deselect all parts
                foreach (var part in quotationService.QuotationServiceParts)
                {
                    part.IsSelected = false;
                }
            }

            await _quotationRepository.UpdateAsync(quotation);
            return true;
        }
        
        /// <summary>
        /// Generates jobs from an approved quotation
        /// </summary>
        /// <param name="quotation">The approved quotation</param>
        private async Task GenerateJobsFromQuotationAsync(Quotation quotation)
        {
            // Get all selected quotation services
            var selectedServices = quotation.QuotationServices.Where(qs => qs.IsSelected).ToList();
            
            foreach (var quotationService in selectedServices)
            {
                // Create a job for each selected service
                var job = new Job
                {
                    ServiceId = quotationService.ServiceId,
                    RepairOrderId = quotation.RepairOrderId ?? Guid.Empty,
                    JobName = $"{quotationService.Service?.ServiceName ?? "Service"} - Quotation {quotation.QuotationId.ToString().Substring(0, 8)}",
                    Status = JobStatus.Pending,
                    TotalAmount = quotationService.Price,
                    Note = $"Auto-generated from approved quotation {quotation.QuotationId}",
                    CreatedAt = DateTime.UtcNow
                };

                // Save the job
                var createdJob = await _jobService.CreateJobAsync(job);
                
                // Create job parts for selected parts
                var selectedParts = quotationService.QuotationServiceParts.Where(qsp => qsp.IsSelected).ToList();
                foreach (var quotationPart in selectedParts)
                {
                    var jobPart = new JobPart
                    {
                        JobId = createdJob.JobId,
                        PartId = quotationPart.PartId,
                        Quantity = (int)quotationPart.Quantity,
                        UnitPrice = quotationPart.Price,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    // Save job part
                    await _jobService.AddJobPartAsync(jobPart);
                }
                
                // Update job total amount after adding parts
                var totalAmount = await _jobService.CalculateJobTotalAmountAsync(createdJob.JobId);
                createdJob.TotalAmount = totalAmount;
                await _jobService.UpdateJobAsync(createdJob);
            }
        }
        
        /// <summary>
        /// Copies an approved quotation to jobs
        /// </summary>
        /// <param name="quotationId">The ID of the quotation to copy to jobs</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> CopyQuotationToJobsAsync(Guid quotationId)
        {
            var quotation = await _quotationRepository.GetByIdAsync(quotationId);
            if (quotation == null)
                throw new ArgumentException($"Quotation with ID {quotationId} not found.");
                
            // Check if quotation is approved
            if (quotation.Status != BusinessObject.Enums.QuotationStatus.Approved)
                throw new InvalidOperationException("Only approved quotations can be copied to jobs.");
            
            // Generate jobs from the quotation
            await GenerateJobsFromQuotationAsync(quotation);
            
            return true;
        }
    }
}