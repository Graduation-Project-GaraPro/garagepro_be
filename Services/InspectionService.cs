﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
using Dtos.Quotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Hubs;
using Services.Notifications;
using Services.QuotationServices;

namespace Services
{
    public class InspectionService : IInspectionService
    {
        private readonly IInspectionRepository _inspectionRepository;
        private readonly IRepairOrderRepository _repairOrderRepository;
        private readonly IQuotationService _quotationService;
        private readonly IHubContext<TechnicianAssignmentHub> _technicianAssignmentHubContext;
        private readonly IHubContext<InspectionHub> _inspectionHubContext;
        private readonly INotificationService _notificationService;
        private readonly DataAccessLayer.MyAppDbContext _dbContext;

        public InspectionService(
            IInspectionRepository inspectionRepository,
            IRepairOrderRepository repairOrderRepository,
            IQuotationService quotationService,
            IHubContext<TechnicianAssignmentHub> technicianAssignmentHubContext,
            IHubContext<InspectionHub> inspectionHubContext,
            INotificationService notificationService,
            DataAccessLayer.MyAppDbContext dbContext)
        {
            _inspectionRepository = inspectionRepository;
            _repairOrderRepository = repairOrderRepository;
            _quotationService = quotationService;
            _technicianAssignmentHubContext = technicianAssignmentHubContext;
            _inspectionHubContext = inspectionHubContext;
            _notificationService = notificationService;
            _dbContext = dbContext;
        }

        public async Task<InspectionDto> GetInspectionByIdAsync(Guid inspectionId)
        {
            var inspection = await _inspectionRepository.GetByIdAsync(inspectionId);
            if (inspection == null) return null;

            var inspectionDto = MapToDto(inspection);

            // Send SignalR notification if inspection is assigned to a technician
            if (inspection.TechnicianId.HasValue)
            {
                await _inspectionHubContext.Clients
                    .Group($"Technician_{inspection.TechnicianId.Value}")
                    .SendAsync("InspectionRetrieved", new
                    {
                        InspectionId = inspectionId,
                        TechnicianId = inspection.TechnicianId.Value,
                        Inspection = inspectionDto,
                        RetrievedAt = DateTime.UtcNow
                    });
            }

            return inspectionDto;
        }

        public async Task<IEnumerable<InspectionDto>> GetAllInspectionsAsync()
        {
            var inspections = await _inspectionRepository.GetAllAsync();
            return inspections.Select(MapToDto);
        }

        public async Task<IEnumerable<InspectionDto>> GetInspectionsByRepairOrderIdAsync(Guid repairOrderId)
        {
            var inspections = await _inspectionRepository.GetByRepairOrderIdAsync(repairOrderId);
            return inspections.Select(MapToDto);
        }

        public async Task<IEnumerable<InspectionDto>> GetInspectionsByTechnicianIdAsync(Guid technicianId)
        {
            var inspections = await _inspectionRepository.GetByTechnicianIdAsync(technicianId);
            var inspectionDtos = inspections.Select(MapToDto).ToList();

            // Send SignalR notification to the technician
            await _inspectionHubContext.Clients
                .Group($"Technician_{technicianId}")
                .SendAsync("InspectionsRetrieved", new
                {
                    TechnicianId = technicianId,
                    InspectionCount = inspectionDtos.Count,
                    Inspections = inspectionDtos,
                    RetrievedAt = DateTime.UtcNow
                });

            return inspectionDtos;
        }

        public async Task<InspectionDto> CreateInspectionAsync(CreateInspectionDto createInspectionDto)
        {
            // Validate that the repair order exists
            var repairOrder = await _repairOrderRepository.GetByIdAsync(createInspectionDto.RepairOrderId);
            if (repairOrder == null)
                throw new ArgumentException("Repair order not found");

            var inspection = new Inspection
            {
                RepairOrderId = createInspectionDto.RepairOrderId,
                CustomerConcern = createInspectionDto.CustomerConcern,
                Status = InspectionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var createdInspection = await _inspectionRepository.CreateAsync(inspection);
            
            // Check if this is the first inspection for this RO
            var existingInspections = await _inspectionRepository.GetByRepairOrderIdAsync(createInspectionDto.RepairOrderId);
            if (existingInspections.Count() == 1) // Only the one we just created
            {
                // Update RO status to In Progress (OrderStatusId = 2)
                await _repairOrderRepository.UpdateRepairOrderStatusAsync(createInspectionDto.RepairOrderId, 2);
            }
            
            return MapToDto(createdInspection);
        }

        public async Task<InspectionDto> CreateManagerInspectionAsync(CreateManagerInspectionDto createManagerInspectionDto)
        {
            // Validate that the repair order exists
            var repairOrder = await _repairOrderRepository.GetByIdAsync(createManagerInspectionDto.RepairOrderId);
            if (repairOrder == null)
                throw new ArgumentException("Repair order not found");

            // Validate services if provided
            if (createManagerInspectionDto.ServiceIds != null && createManagerInspectionDto.ServiceIds.Any())
            {
                // Get all services from COMPLETED inspections only for this RO
                // Note: Services in RO can be inspected again, only completed inspection services are blocked
                var existingInspections = await _inspectionRepository.GetByRepairOrderIdAsync(createManagerInspectionDto.RepairOrderId);
                var completedInspectionServices = existingInspections
                    .Where(i => i.Status == InspectionStatus.Completed)
                    .SelectMany(i => i.ServiceInspections ?? new List<ServiceInspection>())
                    .Select(si => si.ServiceId)
                    .Distinct()
                    .ToList();

                // Check for duplicates with completed inspection services only
                var duplicateServices = createManagerInspectionDto.ServiceIds
                    .Where(sid => completedInspectionServices.Contains(sid))
                    .ToList();

                if (duplicateServices.Any())
                {
                    // Get service names for better error message
                    var duplicateServiceNames = await _dbContext.Services
                        .Where(s => duplicateServices.Contains(s.ServiceId))
                        .Select(s => s.ServiceName)
                        .ToListAsync();

                    throw new InvalidOperationException(
                        $"The following services have already been inspected in completed inspections: {string.Join(", ", duplicateServiceNames)}");
                }
            }

            var inspection = new Inspection
            {
                RepairOrderId = createManagerInspectionDto.RepairOrderId,
                CustomerConcern = createManagerInspectionDto.CustomerConcern,
                Status = InspectionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            
            // Only assign technician if provided
            if (createManagerInspectionDto.TechnicianId.HasValue)
            {
                inspection.TechnicianId = createManagerInspectionDto.TechnicianId.Value;
            }

            var createdInspection = await _inspectionRepository.CreateAsync(inspection);
            
            // Add services to inspection if provided
            if (createManagerInspectionDto.ServiceIds != null && createManagerInspectionDto.ServiceIds.Any())
            {
                foreach (var serviceId in createManagerInspectionDto.ServiceIds)
                {
                    var serviceInspection = new ServiceInspection
                    {
                        InspectionId = createdInspection.InspectionId,
                        ServiceId = serviceId,
                        CreatedAt = DateTime.UtcNow
                        // ConditionStatus will use default value from ServiceInspection model (Not_Checked)
                    };
                    
                    await _dbContext.ServiceInspections.AddAsync(serviceInspection);
                }
                
                await _dbContext.SaveChangesAsync();
            }
            
            // Check if this is the first inspection in ro
            var existingInspectionsAfterCreate = await _inspectionRepository.GetByRepairOrderIdAsync(createManagerInspectionDto.RepairOrderId);
            if (existingInspectionsAfterCreate.Count() == 1)
            {
                await _repairOrderRepository.UpdateRepairOrderStatusAsync(createManagerInspectionDto.RepairOrderId, 2);
            }
            
            var finalInspection = await _inspectionRepository.GetByIdAsync(createdInspection.InspectionId);
            return MapToDto(finalInspection);
        }

        public async Task<InspectionDto> UpdateInspectionAsync(Guid inspectionId, UpdateInspectionDto updateInspectionDto)
        {
            var inspection = await _inspectionRepository.GetByIdAsync(inspectionId);
            if (inspection == null)
                throw new ArgumentException("Inspection not found");

            var oldStatus = inspection.Status;

            if (updateInspectionDto.TechnicianId.HasValue)
            {
                inspection.TechnicianId = updateInspectionDto.TechnicianId.Value;
            }

            if (updateInspectionDto.Status.HasValue)
            {
                inspection.Status = updateInspectionDto.Status.Value;
            }

            if (!string.IsNullOrEmpty(updateInspectionDto.CustomerConcern))
                inspection.CustomerConcern = updateInspectionDto.CustomerConcern;
                
            if (updateInspectionDto.Finding != null)
                inspection.Finding = updateInspectionDto.Finding;
                
            inspection.IssueRating = updateInspectionDto.IssueRating;
            
            if (updateInspectionDto.Note != null)
                inspection.Note = updateInspectionDto.Note;
                
                
            inspection.UpdatedAt = DateTime.UtcNow;

            var updatedInspection = await _inspectionRepository.UpdateAsync(inspection);
            var inspectionDto = MapToDto(updatedInspection);

            // Send SignalR notification if status changed
            if (oldStatus != updatedInspection.Status)
            {
                Console.WriteLine($"[InspectionService] Status changed for Inspection {inspectionId}: {oldStatus} → {updatedInspection.Status}");
                
                var statusMessage = GetStatusChangeMessage(oldStatus, updatedInspection.Status);
                
                var notificationPayload = new
                {
                    InspectionId = inspectionId,
                    TechnicianId = updatedInspection.TechnicianId,
                    TechnicianName = updatedInspection.Technician?.User != null 
                        ? $"{updatedInspection.Technician.User.FirstName} {updatedInspection.Technician.User.LastName}".Trim() 
                        : "Unknown Technician",
                    OldStatus = oldStatus.ToString(),
                    NewStatus = updatedInspection.Status.ToString(),
                    RepairOrderId = updatedInspection.RepairOrderId,
                    CustomerConcern = updatedInspection.CustomerConcern,
                    Finding = updatedInspection.Finding,
                    IssueRating = updatedInspection.IssueRating,
                    Inspection = inspectionDto,
                    Message = statusMessage,
                    UpdatedAt = DateTime.UtcNow
                };

                // Notify the technician (if assigned)
                if (updatedInspection.TechnicianId.HasValue)
                {
                    Console.WriteLine($"[InspectionService] Sending InspectionStatusUpdated to Technician_{updatedInspection.TechnicianId.Value}");
                    await _inspectionHubContext.Clients
                        .Group($"Technician_{updatedInspection.TechnicianId.Value}")
                        .SendAsync("InspectionStatusUpdated", notificationPayload);
                }

                Console.WriteLine($"[InspectionService] Sending InspectionStatusUpdated to Managers group");
                await _inspectionHubContext.Clients
                    .Group("Managers")
                    .SendAsync("InspectionStatusUpdated", notificationPayload);

                Console.WriteLine($"[InspectionService] Sending InspectionStatusUpdated to Inspection_{inspectionId} group");
                await _inspectionHubContext.Clients
                    .Group($"Inspection_{inspectionId}")
                    .SendAsync("InspectionStatusUpdated", notificationPayload);

                if (updatedInspection.Status == InspectionStatus.Completed)
                {
                    Console.WriteLine($"[InspectionService] Inspection {inspectionId} completed by Technician {updatedInspection.TechnicianId}");
                    Console.WriteLine($"[InspectionService] Sending InspectionCompleted to Managers group");

                    await _inspectionHubContext.Clients
                        .Group("Managers")
                        .SendAsync("InspectionCompleted", new
                        {
                            InspectionId = inspectionId,
                            RepairOrderId = updatedInspection.RepairOrderId,
                            TechnicianId = updatedInspection.TechnicianId,
                            TechnicianName = notificationPayload.TechnicianName,
                            CustomerConcern = updatedInspection.CustomerConcern,
                            Finding = updatedInspection.Finding,
                            IssueRating = updatedInspection.IssueRating,
                            ServiceCount = updatedInspection.ServiceInspections?.Count ?? 0,
                            PartCount = updatedInspection.PartInspections?.Count ?? 0,
                            CompletedAt = DateTime.UtcNow,
                            InspectionDetails = inspectionDto,
                            Message = "Inspection completed and ready for quotation"
                        });
                    
                    Console.WriteLine($"[InspectionService] InspectionCompleted event sent successfully");
                }

                // Special notification when inspection starts (InProgress)
                if (updatedInspection.Status == InspectionStatus.InProgress && oldStatus == InspectionStatus.Pending)
                {
                    Console.WriteLine($"[InspectionService] Inspection {inspectionId} started by Technician {updatedInspection.TechnicianId}");
                    Console.WriteLine($"[InspectionService] Sending InspectionStarted to Managers group");
                    
                    // Notify managers that technician has started working
                    await _inspectionHubContext.Clients
                        .Group("Managers")
                        .SendAsync("InspectionStarted", new
                        {
                            InspectionId = inspectionId,
                            RepairOrderId = updatedInspection.RepairOrderId,
                            TechnicianId = updatedInspection.TechnicianId,
                            TechnicianName = notificationPayload.TechnicianName,
                            CustomerConcern = updatedInspection.CustomerConcern,
                            StartedAt = DateTime.UtcNow,
                            Message = "Technician has started the inspection"
                        });
                    
                    Console.WriteLine($"[InspectionService] InspectionStarted event sent successfully");
                }
            }
            else
            {
                Console.WriteLine($"[InspectionService] No status change detected for Inspection {inspectionId}. Status remains: {updatedInspection.Status}");
            }

            return inspectionDto;
        }

        private string GetStatusChangeMessage(InspectionStatus oldStatus, InspectionStatus newStatus)
        {
            return newStatus switch
            {
                InspectionStatus.InProgress => "Inspection is now in progress",
                InspectionStatus.Completed => "Inspection has been completed",
                InspectionStatus.Pending => "Inspection is pending",
                InspectionStatus.New => "New inspection created",
                _ => $"Inspection status changed from {oldStatus} to {newStatus}"
            };
        }

        public async Task<bool> DeleteInspectionAsync(Guid inspectionId)
        {
            return await _inspectionRepository.DeleteAsync(inspectionId);
        }

        public async Task<bool> InspectionExistsAsync(Guid inspectionId)
        {
            return await _inspectionRepository.ExistsAsync(inspectionId);
        }

        public async Task<IEnumerable<InspectionDto>> GetPendingInspectionsAsync()
        {
            var inspections = await _inspectionRepository.GetPendingInspectionsAsync();
            return inspections.Select(MapToDto);
        }

        public async Task<IEnumerable<InspectionDto>> GetCompletedInspectionsAsync()
        {
            var inspections = await _inspectionRepository.GetCompletedInspectionsAsync();
            return inspections.Select(MapToDto);
        }

        public async Task<IEnumerable<CompletedInspectionDto>> GetCompletedInspectionsWithDetailsAsync()
        {
            var inspections = await _inspectionRepository.GetCompletedInspectionsAsync();
            return inspections.Select(MapToCompletedInspectionDto);
        }

        public async Task<bool> AssignInspectionToTechnicianAsync(Guid inspectionId, Guid technicianId)
        {
            // 1. Get inspection details
            var inspection = await _inspectionRepository.GetByIdAsync(inspectionId);
            if (inspection == null)
                throw new ArgumentException("Inspection not found", nameof(inspectionId));

            // 2. Validate inspection status - cannot assign if already in progress
            if (inspection.Status == InspectionStatus.InProgress)
                throw new InvalidOperationException("Cannot assign inspection that is already in progress");

            var technician = await _inspectionRepository.GetTechnicianByIdAsync(technicianId);
            var technicianName = technician?.User != null ? $"{technician.User.FirstName} {technician.User.LastName}".Trim() : "Unknown Technician";

            var result = await _inspectionRepository.AssignInspectionToTechnicianAsync(inspectionId, technicianId);

            if (result)
            {
                await _technicianAssignmentHubContext.Clients.All.SendAsync(
                    "InspectionAssigned",
                    technicianId,
                    technicianName,
                    1,
                    new[] { inspection.CustomerConcern ?? "Unnamed Inspection" }
                );

                await _inspectionHubContext.Clients
                    .Group($"Technician_{technicianId}")
                    .SendAsync("InspectionAssigned", new
                    {
                        InspectionId = inspectionId,
                        TechnicianId = technicianId,
                        TechnicianName = technicianName,
                        CustomerConcern = inspection.CustomerConcern ?? "No concern specified",
                        RepairOrderId = inspection.RepairOrderId,
                        Status = inspection.Status.ToString(),
                        AssignedAt = DateTime.UtcNow,
                        Message = "You have been assigned a new inspection"
                    });

                var userId = await _inspectionRepository.GetUserIdByTechnicianIdAsync(technicianId);

                if (!string.IsNullOrEmpty(userId))
                {
                    await _notificationService.SendInspectionAssignedNotificationAsync(
                        userId,
                        inspectionId,
                        inspection.CustomerConcern ?? "New Inspection",
                        inspection.RepairOrderId
                    );

                    Console.WriteLine($"[InspectionService] Inspection {inspectionId} assigned and notification sent to User {userId}");
                }
            }

            return result;
        }

        public async Task<QuotationDto> ConvertInspectionToQuotationAsync(ConvertInspectionToQuotationDto convertDto)
        {
            var inspection = await _inspectionRepository.GetByIdAsync(convertDto.InspectionId);
            if (inspection == null)
                throw new ArgumentException("Inspection not found");

            if (inspection.Status != InspectionStatus.Completed)
                throw new InvalidOperationException("Only completed inspections can be converted to quotations");

            if (inspection.RepairOrder == null)
                throw new InvalidOperationException("Inspection must have a valid repair order");

            var quotationServices = new List<CreateQuotationServiceDto>();
            
            // Add services from the inspection
            if (inspection.ServiceInspections != null)
            {
                foreach (var serviceInspection in inspection.ServiceInspections)
                {
                    if (serviceInspection.Service == null)
                        continue;

                    bool isGood = serviceInspection.ConditionStatus == ConditionStatus.Good;
                    bool isRequired = serviceInspection.ConditionStatus == ConditionStatus.Replace;
                    

                    var quotationService = new CreateQuotationServiceDto
                    {
                        ServiceId = serviceInspection.ServiceId,
                        IsSelected = false, 
                        IsRequired = isRequired, 
                        IsGood = isGood, 
                        QuotationServiceParts = new List<CreateQuotationServicePartDto>()
                    };
                    
                    if (!isGood)
                    {
                        // Add parts for this service based on ServicePartCategories
                        var servicePartCategoryIds = serviceInspection.Service.ServicePartCategories?.Select(spc => spc.PartCategoryId).ToList() ?? new List<Guid>();
                        var partInspections = inspection.PartInspections?.Where(pi => servicePartCategoryIds.Contains(pi.PartCategoryId)).ToList() ?? new List<PartInspection>();
                        
                        foreach (var partInspection in partInspections)
                        {
                            quotationService.QuotationServiceParts.Add(new CreateQuotationServicePartDto
                            {
                                PartId = partInspection.PartId,
                                IsSelected = false,
                                Quantity = partInspection.Quantity
                            });
                        }
                    }
                    
                    quotationServices.Add(quotationService);
                }
            }

            // Calculate inspection fee based on service complexity
            var createQuotationDto = new CreateQuotationDto
            {
                InspectionId = inspection.InspectionId,
                RepairOrderId = inspection.RepairOrderId,
                UserId = inspection.RepairOrder.UserId,
                VehicleId = inspection.RepairOrder.VehicleId,
                Note = convertDto.Note,
                QuotationServices = quotationServices
            };

            //  calculate service/part totals
            var quotation = await _quotationService.CreateQuotationAsync(createQuotationDto);

            // Calculate inspection fee PER SERVICE based on IsAdvanced flag
            var quotationEntity = await _dbContext.Quotations
                .Include(q => q.QuotationServices)
                    .ThenInclude(qs => qs.Service)
                .Include(q => q.RepairOrder)
                .FirstOrDefaultAsync(q => q.QuotationId == quotation.QuotationId);

            if (quotationEntity != null)
            {
                // Get inspection types
                var basicInspectionType = await _dbContext.InspectionTypes
                    .FirstOrDefaultAsync(it => it.TypeName == "Basic" && it.IsActive);
                var advancedInspectionType = await _dbContext.InspectionTypes
                    .FirstOrDefaultAsync(it => it.TypeName == "Advanced" && it.IsActive);

                if (basicInspectionType != null && advancedInspectionType != null)
                {
                    decimal totalInspectionFee = 0;
                    decimal goodServicesInspectionTotal = 0;

                    // Calculate inspection fee for EACH service based on its IsAdvanced flag
                    foreach (var quotationService in quotationEntity.QuotationServices)
                    {
                        // Determine inspection fee for this specific service
                        decimal serviceInspectionFee = quotationService.Service.IsAdvanced
                            ? advancedInspectionType.InspectionFee
                            : basicInspectionType.InspectionFee;

                        // Add to total inspection fee (for rejection case)
                        totalInspectionFee += serviceInspectionFee;

                        // If service is Good, set its price to inspection fee
                        if (quotationService.IsGood)
                        {
                            quotationService.Price = serviceInspectionFee;
                            goodServicesInspectionTotal += serviceInspectionFee;
                        }
                    }

                    // Store total inspection fee (sum of all services' inspection fees)
                    quotationEntity.InspectionFee = totalInspectionFee;

                    // Add Good services inspection fees to quotation total
                    if (goodServicesInspectionTotal > 0)
                    {
                        quotationEntity.TotalAmount += goodServicesInspectionTotal;
                    }

                    // DO NOT update RO cost here - customer response will handle it
                    // Manager only creates quotation and shows preview to customer

                    await _dbContext.SaveChangesAsync();

                    quotation.InspectionFee = totalInspectionFee;
                    quotation.TotalAmount = quotationEntity.TotalAmount;
                }
            }

            return quotation;
        }

        private InspectionDto MapToDto(Inspection inspection)
        {
            return new InspectionDto
            {
                InspectionId = inspection.InspectionId,
                RepairOrderId = inspection.RepairOrderId,
                TechnicianId = inspection.TechnicianId,
                Status = inspection.Status.ToString(),
                CustomerConcern = inspection.CustomerConcern,
                Finding = inspection.Finding,
                IssueRating = inspection.IssueRating,
                Note = inspection.Note,
                CreatedAt = inspection.CreatedAt,
                UpdatedAt = inspection.UpdatedAt,
                TechnicianName = inspection.Technician?.User != null ? $"{inspection.Technician.User.FirstName} {inspection.Technician.User.LastName}".Trim() : "Unknown Technician",
                Services = inspection.ServiceInspections?.Select(si => new InspectionServiceDto
                {
                    ServiceInspectionId = si.ServiceInspectionId,
                    ServiceId = si.ServiceId,
                    ServiceName = si.Service?.ServiceName ?? "Unknown Service",
                    ConditionStatus = si.ConditionStatus,
                    CreatedAt = si.CreatedAt,
                    Parts = inspection.PartInspections?
                        .Where(pi => si.Service?.ServicePartCategories?.Any(spc => spc.PartCategoryId == pi.PartCategoryId) == true)
                        .Select(pi => new InspectionPartDto
                        {
                            PartInspectionId = pi.PartInspectionId,
                            PartId = pi.PartId,
                            PartName = pi.Part?.Name ?? "Unknown Part",
                            Quantity = pi.Quantity,
                            CreatedAt = pi.CreatedAt
                        }).ToList() ?? new List<InspectionPartDto>()
                }).ToList() ?? new List<InspectionServiceDto>()
            };
        }

        private CompletedInspectionDto MapToCompletedInspectionDto(Inspection inspection)
        {
            return new CompletedInspectionDto
            {
                InspectionId = inspection.InspectionId,
                RepairOrderId = inspection.RepairOrderId,
                TechnicianId = inspection.TechnicianId,
                Status = inspection.Status.ToString(),
                CustomerConcern = inspection.CustomerConcern,
                Finding = inspection.Finding,
                IssueRating = inspection.IssueRating,
                Note = inspection.Note,
                CreatedAt = inspection.CreatedAt,
                UpdatedAt = inspection.UpdatedAt,
                TechnicianName = inspection.Technician?.User != null ? $"{inspection.Technician.User.FirstName} {inspection.Technician.User.LastName}".Trim() : "Unknown Technician",
                Services = inspection.ServiceInspections?.Select(si => new InspectionServiceDto
                {
                    ServiceInspectionId = si.ServiceInspectionId,
                    ServiceId = si.ServiceId,
                    ServiceName = si.Service?.ServiceName ?? "Unknown Service",
                    ConditionStatus = si.ConditionStatus,
                    CreatedAt = si.CreatedAt,
                    Parts = inspection.PartInspections?
                        .Where(pi => si.Service?.ServicePartCategories?.Any(spc => spc.PartCategoryId == pi.PartCategoryId) == true)
                        .Select(pi => new InspectionPartDto
                        {
                            PartInspectionId = pi.PartInspectionId,
                            PartId = pi.PartId,
                            PartName = pi.Part?.Name ?? "Unknown Part",
                            Quantity = pi.Quantity,
                            CreatedAt = pi.CreatedAt
                        }).ToList() ?? new List<InspectionPartDto>()
                }).ToList() ?? new List<InspectionServiceDto>()
            };
        }

        public async Task<IEnumerable<AvailableServiceDto>> GetAvailableServicesForInspectionAsync(Guid repairOrderId)
        {
            // Get all services from COMPLETED inspections only for this RO
            var existingInspections = await _inspectionRepository.GetByRepairOrderIdAsync(repairOrderId);
            var completedInspectionServices = existingInspections
                .Where(i => i.Status == InspectionStatus.Completed)
                .SelectMany(i => i.ServiceInspections ?? new List<ServiceInspection>())
                .Select(si => si.ServiceId)
                .Distinct()
                .ToList();

            // k dc chon service trong inspection completed
            var availableServices = await _dbContext.Services
                .Where(s => s.IsActive && !completedInspectionServices.Contains(s.ServiceId))
                .Select(s => new AvailableServiceDto
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    Description = s.Description,
                    Price = s.Price,
                    IsAdvanced = s.IsAdvanced,
                    ServiceCategoryId = s.ServiceCategoryId,
                    ServiceCategoryName = s.ServiceCategory != null ? s.ServiceCategory.CategoryName : null
                })
                .OrderBy(s => s.ServiceCategoryName)
                .ThenBy(s => s.ServiceName)
                .ToListAsync();

            return availableServices;
        }
    }
}