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
using Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Services.Hubs;
using Services.FCMServices;
using BusinessObject.FcmDataModels; // Add this using statement

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
        private readonly IHubContext<QuotationHub> _quotationHubContext;
        private readonly IFcmService _fcmService;
        private readonly IUserService _userService;
        private readonly Services.Notifications.INotificationService _notificationService;
        public QuotationManagementService(
        IQuotationRepository quotationRepository,
        IQuotationServiceRepository quotationServiceRepository,
        IQuotationServicePartRepository quotationServicePartRepository,
        IMapper mapper)
        : this(
            quotationRepository,
            quotationServiceRepository,
            quotationServicePartRepository,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            mapper)
        {
        }

        // Constructor đầy đủ dùng cho DI chính
        public QuotationManagementService(
            IQuotationRepository quotationRepository,
            IQuotationServiceRepository quotationServiceRepository,
            IQuotationServicePartRepository quotationServicePartRepository,
            IServiceRepository serviceRepository,
            IPartRepository partRepository,
            IHubContext<QuotationHub> quotationHubContext,
            IRepairOrderRepository repairOrderRepository,
            IJobService jobService, 
            // Add this parameter
             IFcmService fcmService,
            IUserService userService,
            Services.Notifications.INotificationService notificationService,
        IMapper mapper)
        {
            _quotationRepository = quotationRepository;
            _quotationServiceRepository = quotationServiceRepository;
            _quotationServicePartRepository = quotationServicePartRepository;
            _serviceRepository = serviceRepository;
            _partRepository = partRepository;
            _quotationHubContext = quotationHubContext;
            _repairOrderRepository = repairOrderRepository;
            _jobService = jobService;
            _fcmService = fcmService;
            _userService = userService;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task<QuotationDto> CreateQuotationAsync(CreateQuotationDto createQuotationDto)
        {
            // If RepairOrderId is provided, get UserId and VehicleId from the RepairOrder
            string userId = createQuotationDto.UserId;
            Guid vehicleId = createQuotationDto.VehicleId;

            if (createQuotationDto.RepairOrderId.HasValue)
            {
                // Multiple quotations are now allowed for the same repair order
                // This enables creating alternative quotes with different service/part combinations
                
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
            bool allServicesGood = true; // Track if all services are Good
            
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

                    // Track if any service is NOT Good
                    if (!serviceDto.IsGood)
                    {
                        allServicesGood = false;
                    }

                    var quotationService = new QuotationService // entity
                    {
                        QuotationId = createdQuotation.QuotationId,
                        ServiceId = serviceDto.ServiceId,
                        IsSelected = serviceDto.IsSelected,
                        IsRequired = serviceDto.IsRequired, // Set IsRequired 
                        IsGood = serviceDto.IsGood, // Set IsGood
                        Price = service.Price // Store the actual service price
                    };

                    await _quotationServiceRepository.CreateAsync(quotationService);

                    // Create quotation service parts for this service when service is NOT Good
                    if (!serviceDto.IsGood && serviceDto.QuotationServiceParts != null)
                    {
                        foreach (var partDto in serviceDto.QuotationServiceParts)
                        {
                            // Get the actual part to retrieve price
                            var part = await _partRepository.GetByIdAsync(partDto.PartId);
                            if (part == null)
                            {
                                throw new ArgumentException($"Part with ID {partDto.PartId} not found.");
                            }

                            var quotationServicePart = new QuotationServicePart
                            {
                                QuotationServiceId = quotationService.QuotationServiceId,
                                PartId = partDto.PartId,
                                IsSelected = partDto.IsSelected, // Use the IsSelected from DTO (goi y part cua tech)
                                Price = part.Price,
                                Quantity = partDto.Quantity
                            };

                            await _quotationServicePartRepository.CreateAsync(quotationServicePart);
                            // Don't add to the collection directly, let the repository handle the relationship
                        }
                    }
                }
            }

            // For normal quotations (not all services Good), set TotalAmount to 0 before customer response
            // Total will be calculated only after customer selects services and parts
            if (!allServicesGood)
            {
                totalAmount = 0;
            }

            // Update the quotation with the calculated total amount and status
            createdQuotation.TotalAmount = totalAmount;
            
            // If all services are Good, set quotation status to Good (view-only, no payment)
            if (allServicesGood && createQuotationDto.QuotationServices.Any())
            {
                createdQuotation.Status = QuotationStatus.Good;

                await _quotationHubContext
                       .Clients
                       .Group($"User_{createQuotationDto.UserId}")
                       .SendAsync("QuotationCreated", new
                       {
                           createdQuotation.QuotationId,
                           createdQuotation.UserId,
                           createdQuotation.RepairOrderId,
                           createdQuotation.TotalAmount,
                           createdQuotation.Status,
                           createdQuotation.CreatedAt,
                           createQuotationDto.Note
                       });

                var user = await _userService.GetUserByIdAsync(createdQuotation.UserId);

                if (user != null && user.DeviceId != null)
                {
                    var FcmNotification = new FcmDataPayload
                    {
                        Type = NotificationType.Repair,
                        Title = "New Quotation Available",
                        Body = "A new quotation has been created for your repair job. Tap to view details.",
                        EntityKey = EntityKeyType.quotationId,
                        EntityId = createdQuotation.QuotationId,
                        Screen = AppScreen.QuotationDetailFragment
                    };
                    await _fcmService.SendFcmMessageAsync(user.DeviceId, FcmNotification);
                }
            }
            
            await _quotationRepository.UpdateAsync(createdQuotation);

            // Auto-update repair order status to "In Progress"
            if (createQuotationDto.RepairOrderId.HasValue)
            {
                var repairOrder = await _repairOrderRepository.GetByIdAsync(createQuotationDto.RepairOrderId.Value);
                if (repairOrder != null)
                {
                    if (repairOrder.StatusId == 1) 
                    {
                        await _repairOrderRepository.UpdateRepairOrderStatusAutomaticAsync(
                            createQuotationDto.RepairOrderId.Value, 
                            2 
                        );
                    }
                    else if (repairOrder.StatusId == 3) 
                    {                       
                        // Only move back to In Progress if not paid
                        if (repairOrder.PaidStatus == PaidStatus.Unpaid)
                        {
                            await _repairOrderRepository.UpdateRepairOrderStatusAutomaticAsync(
                                createQuotationDto.RepairOrderId.Value, 
                                2
                            );
                        }
                    }
                }
            }




           


            // Reload the quotation with all related data to ensure we have the complete object
            var completeQuotation = await _quotationRepository.GetByIdAsync(createdQuotation.QuotationId);
            return _mapper.Map<QuotationDto>(completeQuotation);
        }


        public async Task<object> GetQuotationsByUserIdAsync(
            string userId,
            int pageNumber,
            int pageSize,
            QuotationStatus? status)
        {
            var (quotations, totalCount) = await _quotationRepository
                .GetQuotationsByUserIdAsync(userId, pageNumber, pageSize, status);

            var quotationDtos = _mapper.Map<List<QuotationDto>>(quotations);

            return new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Data = quotationDtos
            };
        }


        public async Task<QuotationDetailDto> GetQuotationDetailByIdAsync(Guid quotationId)
        {
            var quotation = await _quotationRepository.GetByIdAsync(quotationId);

            if (quotation == null)
                throw new Exception("Not Found Quotation");

            var dto = _mapper.Map<QuotationDetailDto>(quotation);

            // Tạo danh sách PartCategories cho từng service
            foreach (var serviceDto in dto.QuotationServices)
            {
                var serviceEntity = quotation.QuotationServices.First(x => x.QuotationServiceId == serviceDto.QuotationServiceId);

                serviceDto.PartCategories = serviceEntity.QuotationServiceParts
                                            .Where(p => p.Part != null && p.Part.PartCategory != null) // tránh null
                                            .GroupBy(p => new
                                            {
                                                p.Part.PartCategoryId,
                                                CategoryName = p.Part.PartCategory.CategoryName
                                            })
                                            .Select(g => new QuotationPartCategoryDTO
                                            {
                                                PartCategoryId = g.Key.PartCategoryId,
                                                PartCategoryName = g.Key.CategoryName,
                                                Parts = g.Select(p => new QuotationPart
                                                {
                                                    QuotationServicePartId = p.QuotationServicePartId,
                                                    PartId = p.PartId,
                                                    PartName = p.Part?.Name ?? "Unknown Part",
                                                    Price = p.Price,
                                                    Quantity = p.Quantity,
                                                    WarrantyMonths = p?.Part?.WarrantyMonths,
                                                    IsSelected = p.IsSelected
                                                }).ToList()
                                            }).ToList();
            }

           

            return dto;
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


            await _quotationHubContext
            .Clients
            .Group($"User_{updatedQuotation.UserId}")
            .SendAsync("QuotationCreated", new
            {
                updatedQuotation.QuotationId,
                updatedQuotation.UserId,
                updatedQuotation.RepairOrderId,
                updatedQuotation.TotalAmount,
                updatedQuotation.Status,
                updatedQuotation.CreatedAt,
                updatedQuotation.Note
            });

            var user = await _userService.GetUserByIdAsync(updatedQuotation.UserId);

            if (user != null && user.DeviceId != null)
            {
                var FcmNotification = new FcmDataPayload
                {
                    Type = NotificationType.Repair,
                    Title = "Quotation Available",
                    Body = "A new quotation has been created for your repair job. Tap to view details.",
                    EntityKey = EntityKeyType.quotationId,
                    EntityId = updatedQuotation.QuotationId,
                    Screen = AppScreen.QuotationDetailFragment
                };
                await _fcmService.SendFcmMessageAsync(user.DeviceId, FcmNotification);
            }

            return _mapper.Map<QuotationDto>(updatedQuotation);
        }

        public async Task<QuotationDetailDto> UpdateQuotationDetailsAsync(Guid quotationId, UpdateQuotationDetailsDto updateDto)
        {
            var existingQuotation = await _quotationRepository.GetByIdAsync(quotationId);
            if (existingQuotation == null)
                throw new ArgumentException($"Quotation with ID {quotationId} not found.");

            // Update basic quotation fields
            if (updateDto.Note != null)
                existingQuotation.Note = updateDto.Note;
            
            if (updateDto.ExpiresAt.HasValue)
                existingQuotation.ExpiresAt = updateDto.ExpiresAt.Value;
            
            if (updateDto.DiscountAmount.HasValue)
                existingQuotation.DiscountAmount = updateDto.DiscountAmount.Value;

            existingQuotation.UpdatedAt = DateTime.UtcNow;

            // Update services and parts
            if (updateDto.QuotationServices != null)
            {
                foreach (var serviceDto in updateDto.QuotationServices)
                {
                    // Delete service if flagged
                    if (serviceDto.ShouldDelete && serviceDto.QuotationServiceId.HasValue)
                    {
                        var serviceToDelete = existingQuotation.QuotationServices
                            .FirstOrDefault(qs => qs.QuotationServiceId == serviceDto.QuotationServiceId.Value);
                        
                        if (serviceToDelete != null)
                        {
                            // Delete all parts first
                            var partsToDelete = serviceToDelete.QuotationServiceParts.ToList();
                            foreach (var part in partsToDelete)
                            {
                                await _quotationServicePartRepository.DeleteAsync(part.QuotationServicePartId);
                            }
                            
                            // Delete the service
                            await _quotationServiceRepository.DeleteAsync(serviceToDelete.QuotationServiceId);
                        }
                        continue;
                    }

                    // Update existing service
                    if (serviceDto.QuotationServiceId.HasValue)
                    {
                        var existingService = existingQuotation.QuotationServices
                            .FirstOrDefault(qs => qs.QuotationServiceId == serviceDto.QuotationServiceId.Value);
                        
                        if (existingService != null)
                        {
                            // Manager can update service selection status
                            existingService.IsSelected = serviceDto.IsSelected;
                            // Note: IsRequired cannot be changed - it's set during inspection
                            await _quotationServiceRepository.UpdateAsync(existingService);

                            // Update parts for this service
                            if (serviceDto.QuotationServiceParts != null)
                            {
                                foreach (var partDto in serviceDto.QuotationServiceParts)
                                {
                                    // Delete part if flagged
                                    if (partDto.ShouldDelete && partDto.QuotationServicePartId.HasValue)
                                    {
                                        await _quotationServicePartRepository.DeleteAsync(partDto.QuotationServicePartId.Value);
                                        continue;
                                    }

                                    // Update existing part
                                    if (partDto.QuotationServicePartId.HasValue)
                                    {
                                        var existingPart = existingService.QuotationServiceParts
                                            .FirstOrDefault(qsp => qsp.QuotationServicePartId == partDto.QuotationServicePartId.Value);
                                        
                                        if (existingPart != null)
                                        {
                                            // Manager can update quantity and pre-selection (recommendation)
                                            existingPart.Quantity = partDto.Quantity;
                                            existingPart.IsSelected = partDto.IsSelected;
                                            await _quotationServicePartRepository.UpdateAsync(existingPart);
                                        }
                                    }
                                    // Add new part
                                    else
                                    {
                                        var part = await _partRepository.GetByIdAsync(partDto.PartId);
                                        if (part == null)
                                            throw new ArgumentException($"Part with ID {partDto.PartId} not found.");

                                        var newPart = new QuotationServicePart
                                        {
                                            QuotationServiceId = existingService.QuotationServiceId,
                                            PartId = partDto.PartId,
                                            IsSelected = partDto.IsSelected, // Manager can pre-select (recommend)
                                            Price = part.Price,
                                            Quantity = partDto.Quantity
                                        };
                                        
                                        await _quotationServicePartRepository.CreateAsync(newPart);
                                    }
                                }
                            }
                        }
                    }
                    // Add new service
                    else
                    {
                        var service = await _serviceRepository.GetByIdAsync(serviceDto.ServiceId);
                        if (service == null)
                            throw new ArgumentException($"Service with ID {serviceDto.ServiceId} not found.");

                        var newService = new QuotationService
                        {
                            QuotationId = existingQuotation.QuotationId,
                            ServiceId = serviceDto.ServiceId,
                            IsSelected = serviceDto.IsSelected,
                            IsRequired = false, // Default to false for new services added by manager
                            Price = service.Price
                        };
                        
                        var createdService = await _quotationServiceRepository.CreateAsync(newService);

                        // Add parts for the new service
                        if (serviceDto.QuotationServiceParts != null)
                        {
                            foreach (var partDto in serviceDto.QuotationServiceParts)
                            {
                                if (partDto.ShouldDelete) continue;

                                var part = await _partRepository.GetByIdAsync(partDto.PartId);
                                if (part == null)
                                    throw new ArgumentException($"Part with ID {partDto.PartId} not found.");

                                var newPart = new QuotationServicePart
                                {
                                    QuotationServiceId = createdService.QuotationServiceId,
                                    PartId = partDto.PartId,
                                    IsSelected = partDto.IsSelected, // Manager can pre-select (recommend)
                                    Price = part.Price,
                                    Quantity = partDto.Quantity
                                };
                                
                                await _quotationServicePartRepository.CreateAsync(newPart);
                            }
                        }
                    }
                }
            }

            // Recalculate total amount
            var updatedQuotation = await _quotationRepository.GetByIdAsync(quotationId);
            decimal totalAmount = 0;
            
            foreach (var qs in updatedQuotation.QuotationServices)
            {
                if (qs.IsSelected)
                {
                    totalAmount += qs.Price;
                    
                    foreach (var part in qs.QuotationServiceParts)
                    {
                        if (part.IsSelected)
                        {
                            totalAmount += part.Price * part.Quantity;
                        }
                    }
                }
            }
            
            updatedQuotation.TotalAmount = totalAmount;
            await _quotationRepository.UpdateAsync(updatedQuotation);

            // Return the updated quotation with full details
            return await GetQuotationDetailByIdAsync(quotationId);
        }

        public async Task<bool> DeleteQuotationAsync(Guid quotationId)
        {
            return await _quotationRepository.DeleteAsync(quotationId);
        }

        public async Task<bool> QuotationExistsAsync(Guid quotationId)
        {
            return await _quotationRepository.ExistsAsync(quotationId);
        }

        // REMOVED: ProcessCustomerResponseAsync - Use CustomerResponseQuotationService instead
        // This method has been deprecated in favor of CustomerResponseQuotationService.ProcessCustomerResponseAsync
        // which includes proper validation, promotion handling, and transaction management



        /// Validates and corrects part selection based on whether services are advanced or not.
        private async Task ValidateAndCorrectPartSelectionAsync(Quotation quotation)
        {
            foreach (var quotationService in quotation.QuotationServices)
            {
                // Load the full service information to check if it's advanced
                var service = await _serviceRepository.GetByIdAsync(quotationService.ServiceId);

                if (service != null)
                {
                    // Get all selected parts for this service
                    var selectedParts = quotationService.QuotationServiceParts
                        .Where(qsp => qsp.IsSelected)
                        .ToList();

                    // If it's not an advanced service, ensure only one part is selected
                    if (!service.IsAdvanced && selectedParts.Count > 1)
                    {
                        // Keep only the first selected part and deselect the rest
                        for (int i = 1; i < selectedParts.Count; i++)
                        {
                            selectedParts[i].IsSelected = false;
                        }
                    }

                }
            }
        }


        public async Task<bool> ApproveQuotationAsync(Guid quotationId)
        {
            Console.WriteLine($"ApproveQuotationAsync called with ID: {quotationId}");
            var quotation = await _quotationRepository.GetByIdAsync(quotationId);
            if (quotation == null)
            {
                Console.WriteLine($"Quotation not found for ID: {quotationId}");
                throw new ArgumentException($"Quotation with ID {quotationId} not found.");
            }

            Console.WriteLine($"Approving quotation ID: {quotation.QuotationId}");
            quotation.Status = BusinessObject.Enums.QuotationStatus.Approved;
            quotation.CustomerResponseAt = DateTime.UtcNow;

            // When approving without customer response (direct approval), select all required services and their parts
            // This is a fallback - normally customer should use ProcessCustomerResponseAsync
            foreach (var quotationService in quotation.QuotationServices)
            {
                // Ensure required services are selected
                if (quotationService.IsRequired)
                {
                    quotationService.IsSelected = true;
                    Console.WriteLine($"Selected required service ID: {quotationService.ServiceId}");
                    
                    // Select all parts for required services
                    foreach (var part in quotationService.QuotationServiceParts)
                    {
                        part.IsSelected = true;
                    }
                }
                // For optional services, keep customer's selection (don't change IsSelected)
                // For parts in selected services, keep customer's selection (don't change IsSelected)
            }

            await _quotationRepository.UpdateAsync(quotation);         
            
            Console.WriteLine($"Quotation approved successfully");
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


        // Generates jobs from an approved quotation
        private async Task GenerateJobsFromQuotationAsync(Quotation quotation)
        {
            // Debug: Log quotation information
            Console.WriteLine($"Generating jobs for quotation ID: {quotation.QuotationId}");
            Console.WriteLine($"Quotation services count: {quotation.QuotationServices?.Count() ?? 0}");

            // Get all selected quotation services
            var selectedServices = quotation.QuotationServices.Where(qs => qs.IsSelected).ToList();

            Console.WriteLine($"Selected services count: {selectedServices.Count}");

            if (!selectedServices.Any())
            {
                throw new InvalidOperationException("No selected services found in the quotation.");
            }

            // Validate Repair Order ID
            if (quotation.RepairOrderId == null || quotation.RepairOrderId == Guid.Empty)
            {
                throw new InvalidOperationException("Repair Order ID is required to create jobs.");
            }

            foreach (var quotationService in selectedServices)
            {
                // Debug: Log service information
                Console.WriteLine($"Processing service ID: {quotationService.ServiceId}");
                Console.WriteLine($"Service name: {quotationService.Service?.ServiceName ?? "Unknown"}");
                Console.WriteLine($"Quotation service parts count: {quotationService.QuotationServiceParts?.Count() ?? 0}");

                // Create a job for each selected service
                var job = new Job
                {
                    ServiceId = quotationService.ServiceId,
                    RepairOrderId = quotation.RepairOrderId.Value,
                    JobName = $"{quotationService.Service?.ServiceName ?? "Service"} - Quotation {quotation.QuotationId.ToString().Substring(0, 8)}",
                    Status = JobStatus.Pending,
                    TotalAmount = quotationService.Price,
                    Note = $"Auto-generated from approved quotation",
                    CreatedAt = DateTime.UtcNow
                };

                // Create job parts for selected parts
                var selectedParts = quotationService.QuotationServiceParts.Where(qsp => qsp.IsSelected).ToList();
                Console.WriteLine($"Selected parts count: {selectedParts.Count}");

                var jobParts = new List<JobPart>();
                foreach (var quotationPart in selectedParts)
                {
                    var jobPart = new JobPart
                    {
                        PartId = quotationPart.PartId,
                        Quantity = (int)quotationPart.Quantity,
                        UnitPrice = quotationPart.Price,
                        CreatedAt = DateTime.UtcNow
                    };

                    jobParts.Add(jobPart);
                    Console.WriteLine($"Added part ID: {quotationPart.PartId} to job parts list");
                }

                // Save the job with all its parts in a transaction
                var createdJob = await _jobService.CreateJobWithPartsAsync(job, jobParts);
                Console.WriteLine($"Created job ID: {createdJob.JobId} with {jobParts.Count} parts");
            }
        }

 
        // Copies an approved quotation to jobs

        public async Task<bool> CopyQuotationToJobsAsync(Guid quotationId)
        {
            // Load quotation with all necessary navigation properties
            // Note: The GetByIdAsync method in QuotationRepository already includes all necessary navigation properties
            var quotation = await _quotationRepository.GetByIdAsync(quotationId);
            if (quotation == null)
            {
                throw new ArgumentException($"Quotation with ID {quotationId} not found.");
            }

            Console.WriteLine($"Retrieved quotation ID: {quotation.QuotationId}");
            Console.WriteLine($"Quotation status: {quotation.Status}");
            Console.WriteLine($"Quotation services count: {quotation.QuotationServices?.Count() ?? 0}");
            Console.WriteLine($"Quotation RepairOrderId: {quotation.RepairOrderId}");

            // Check if quotation is approved
            if (quotation.Status != BusinessObject.Enums.QuotationStatus.Approved)
            {
                throw new InvalidOperationException("Only approved quotations can be copied to jobs.");
            }

            // Validation 1: Check if this quotation has already been copied to jobs
            var existingJobsFromQuotation = await _jobService.GetJobsByRepairOrderIdAsync(quotation.RepairOrderId.Value);
            var quotationNote = $"Auto-generated from approved quotation {quotation.QuotationId}";
            
            if (existingJobsFromQuotation.Any(j => j.Note != null && j.Note.Contains(quotation.QuotationId.ToString())))
            {
                throw new InvalidOperationException($"This quotation has already been copied to jobs. Each quotation can only create jobs once.");
            }

            // Validation 2: Check for duplicate services in existing jobs
            var selectedServices = quotation.QuotationServices.Where(qs => qs.IsSelected).ToList();
            var existingServiceIds = existingJobsFromQuotation.Select(j => j.ServiceId).ToHashSet();
            
            var duplicateServices = selectedServices
                .Where(qs => existingServiceIds.Contains(qs.ServiceId))
                .Select(qs => qs.Service?.ServiceName ?? "Unknown Service")
                .ToList();

            if (duplicateServices.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot create jobs: The following services already exist in jobs: {string.Join(", ", duplicateServices)}. " +
                    $"Each service can only have one job in the system.");
            }

            try
            {
                // Generate jobs from the quotation
                await GenerateJobsFromQuotationAsync(quotation);
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error copying quotation to jobs: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to copy quotation to jobs: {ex.Message}", ex);
            }
        }

        /// Creates revision jobs for an updated quotation

        public async Task<bool> CreateRevisionJobsAsync(Guid quotationId, string revisionReason)
        {
            var quotation = await _quotationRepository.GetByIdAsync(quotationId);
            if (quotation == null)
                throw new ArgumentException($"Quotation with ID {quotationId} not found.");

            // Check if quotation is approved
            if (quotation.Status != BusinessObject.Enums.QuotationStatus.Approved)
                throw new InvalidOperationException("Only approved quotations can be used to create revision jobs.");

            // Generate jobs from the quotation (these will be the revision jobs)
            await GenerateJobsFromQuotationAsync(quotation);

            return true;
        }


        // Checks if a repair order can be completed (all quotations are Good OR all quotations are Rejected, and repair order is In Progress)
        public async Task<bool> CanCompleteRepairOrderAsync(Guid repairOrderId)
        {
            try
            {
                // Get all quotations for this repair order
                var quotations = await _quotationRepository.GetByRepairOrderIdAsync(repairOrderId);
                
                if (!quotations.Any())
                {
                    return false;
                }

                // Check if ALL quotations are Good status OR ALL quotations are Rejected status
                var allQuotationsAreGood = quotations.All(q => q.Status == QuotationStatus.Good);
                var allQuotationsAreRejected = quotations.All(q => q.Status == QuotationStatus.Rejected);
                
                if (!allQuotationsAreGood && !allQuotationsAreRejected)
                {
                    return false;
                }

                // Get the repair order to check current status
                var repairOrder = await _repairOrderRepository.GetByIdAsync(repairOrderId);
                
                // Can complete if repair order exists and is In Progress
                return repairOrder != null && repairOrder.StatusId == 2; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to check if RepairOrder {repairOrderId} can be completed: {ex.Message}");
                return false;
            }
        }


        // Manually completes a repair order when all quotations are Good OR all quotations are Rejected
        public async Task CompleteRepairOrderWithGoodQuotationsAsync(Guid repairOrderId)
        {
            // First check if it can be completed
            var canComplete = await CanCompleteRepairOrderAsync(repairOrderId);
            if (!canComplete)
            {
                throw new InvalidOperationException("Repair order cannot be completed. Either not all quotations are Good/Rejected or repair order is not In Progress.");
            }

            // Get quotations and repair order
            var quotations = await _quotationRepository.GetByRepairOrderIdAsync(repairOrderId);
            var repairOrder = await _repairOrderRepository.GetByIdAsync(repairOrderId);

            if (repairOrder == null)
            {
                throw new ArgumentException("Repair order not found");
            }

            // Update repair order to Completed status
            await _repairOrderRepository.UpdateRepairOrderStatusAutomaticAsync(repairOrderId, 3); 
            
            // Set completion date and repair order status
            repairOrder.CompletionDate = DateTime.UtcNow;
            repairOrder.StatusId = 3;

            // Calculate total cost based on quotation status
            var allQuotationsAreGood = quotations.All(q => q.Status == QuotationStatus.Good);
            var allQuotationsAreRejected = quotations.All(q => q.Status == QuotationStatus.Rejected);
            
            decimal totalCost = 0;
            
            if (allQuotationsAreGood)
            {
                // For Good quotations: use inspection fees from quotations
                totalCost = quotations.Sum(q => q.InspectionFee);
            }
            else if (allQuotationsAreRejected)
            {
                // For Rejected quotations: use inspection fees (customer pays for inspection only)
                totalCost = quotations.Sum(q => q.InspectionFee);
            }
            
            repairOrder.Cost = totalCost;
            repairOrder.PaidAmount = totalCost;
            
            await _repairOrderRepository.UpdateAsync(repairOrder);

            // Send notification to managers about manual RO completion
            if (_notificationService != null)
            {
                try
                {
                    // Get notification info using lightweight query
                    var notificationInfo = await _repairOrderRepository.Context.RepairOrders
                        .Where(ro => ro.RepairOrderId == repairOrderId)
                        .Select(ro => new
                        {
                            BranchId = ro.BranchId,
                            CustomerName = (ro.User.FirstName + " " + ro.User.LastName).Trim(),
                            VehicleInfo = ro.Vehicle.Brand.BrandName + " " + ro.Vehicle.Model.ModelName + " (" + ro.Vehicle.LicensePlate + ")"
                        })
                        .FirstOrDefaultAsync();

                    if (notificationInfo != null)
                    {
                        var customerName = string.IsNullOrEmpty(notificationInfo.CustomerName) ? "Unknown Customer" : notificationInfo.CustomerName;
                        var vehicleInfo = string.IsNullOrEmpty(notificationInfo.VehicleInfo) || notificationInfo.VehicleInfo == " ()" ? "Unknown Vehicle" : notificationInfo.VehicleInfo;

                        await _notificationService.SendRepairOrderCompletedNotificationToManagersAsync(
                            repairOrderId,
                            notificationInfo.BranchId,
                            customerName,
                            vehicleInfo,
                            false // false = manually completed
                        );

                        Console.WriteLine($"[QuotationService] Manual completion notification sent for RepairOrder {repairOrderId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[QuotationService] Failed to send completion notification: {ex.Message}");
                }
            }
        }
    }
}