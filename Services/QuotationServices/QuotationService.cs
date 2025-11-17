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
            IJobService jobService, // Add this parameter
             IFcmService fcmService,
            IUserService userService,
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
            _userService= userService;
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

            //if (quotation == null)
            //    return new Exception("Not Found Quotation");

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
                                                    IsSelected = p.IsSelected
                                                }).ToList()
                                            }).ToList();
            }

            //return Ok(dto);

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
            // Lấy quotation cùng toàn bộ dữ liệu liên quan
            var quotation = await _quotationRepository.GetByIdAsync(responseDto.QuotationId);
            if (quotation == null)
                throw new ArgumentException($"Quotation with ID {responseDto.QuotationId} not found.");

            // Cập nhật trạng thái và thời gian phản hồi của khách hàng
            quotation.Status = Enum.Parse<QuotationStatus>(responseDto.Status);
            quotation.CustomerResponseAt = DateTime.UtcNow;
            quotation.Note = responseDto.CustomerNote;
            // Cập nhật lựa chọn dịch vụ (QuotationServices)
            if (responseDto.SelectedServices != null && responseDto.SelectedServices.Any())
            {
                var selectedServiceIds = responseDto.SelectedServices
                    .Select(s => s.QuotationServiceId)
                    .ToHashSet();

                foreach (var qs in quotation.QuotationServices)
                {
                    qs.IsSelected = selectedServiceIds.Contains(qs.QuotationServiceId);
                }
            }
            else
            {
                // Nếu không có dịch vụ nào được gửi lên, có thể giữ nguyên hoặc bỏ chọn tất cả
                foreach (var qs in quotation.QuotationServices)
                {
                    qs.IsSelected = false;
                }
            }

            // Cập nhật lựa chọn phụ tùng (QuotationServiceParts)
            //if (responseDto.SelectedServiceParts != null && responseDto.SelectedServiceParts.Any())
            //{
            //    var selectedPartIds = responseDto.SelectedServiceParts
            //        .Select(p => p.QuotationServicePartId)
            //        .ToHashSet();

            //    foreach (var part in quotation.QuotationServices.SelectMany(qs => qs.QuotationServiceParts))
            //    {
            //        part.IsSelected = selectedPartIds.Contains(part.QuotationServicePartId);
            //    }
            //}
            //else
            //{
            //    // Nếu không có part nào được chọn, bỏ chọn tất cả
            //    foreach (var part in quotation.QuotationServices.SelectMany(qs => qs.QuotationServiceParts))
            //    {
            //        part.IsSelected = false;
            //    }
            //}

            // Kiểm tra và điều chỉnh lựa chọn phụ tùng nếu cần
            //await ValidateAndCorrectPartSelectionAsync(quotation);


            // Lưu lại thay đổi
            var updatedQuotation = await _quotationRepository.UpdateAsync(quotation);

            await _quotationHubContext
                .Clients
                .Group($"Quotation_{updatedQuotation.QuotationId}")
                .SendAsync("QuotationUpdated", new
                {
                    updatedQuotation.QuotationId,
                    updatedQuotation.UserId,
                    updatedQuotation.RepairOrderId,
                    updatedQuotation.TotalAmount,
                    updatedQuotation.Status,
                    updatedQuotation.Note,
                    UpdatedAt = updatedQuotation.UpdatedAt ?? DateTime.UtcNow
                });


            return _mapper.Map<QuotationDto>(updatedQuotation);
        }



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

            // When approving, ensure required services are selected and select all parts for selected services
            foreach (var quotationService in quotation.QuotationServices)
            {
                // Ensure required services are selected
                if (quotationService.IsRequired)
                {
                    quotationService.IsSelected = true;
                    Console.WriteLine($"Selected required service ID: {quotationService.ServiceId}");
                }
                
                // Select all parts for selected services
                if (quotationService.IsSelected)
                {
                    foreach (var part in quotationService.QuotationServiceParts)
                    {
                        part.IsSelected = true;
                    }
                    Console.WriteLine($"Selected {quotationService.QuotationServiceParts.Count} parts for service ID: {quotationService.ServiceId}");
                }
            }

            await _quotationRepository.UpdateAsync(quotation);
            
            // Note: We no longer auto-generate jobs here as per new requirements
            // Jobs will be manually created by manager using CopyQuotationToJobsAsync
            
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
        
        /// <summary>
        /// Generates jobs from an approved quotation
        /// </summary>
        /// <param name="quotation">The approved quotation</param>
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
                    Note = $"Auto-generated from approved quotation {quotation.QuotationId}",
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
        
        /// <summary>
        /// Copies an approved quotation to jobs
        /// </summary>
        /// <param name="quotationId">The ID of the quotation to copy to jobs</param>
        /// <returns>True if successful, false otherwise</returns>
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
        
        /// <summary>
        /// Creates revision jobs for an updated quotation
        /// </summary>
        /// <param name="quotationId">The ID of the quotation</param>
        /// <param name="revisionReason">The reason for the revision</param>
        /// <returns>True if successful, false otherwise</returns>
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
    }
}