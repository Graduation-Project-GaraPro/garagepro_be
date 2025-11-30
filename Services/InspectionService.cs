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

            return MapToDto(inspection);
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
            return inspections.Select(MapToDto);
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
            return MapToDto(createdInspection);
        }

        public async Task<InspectionDto> CreateManagerInspectionAsync(CreateManagerInspectionDto createManagerInspectionDto)
        {
            // Validate that the repair order exists
            var repairOrder = await _repairOrderRepository.GetByIdAsync(createManagerInspectionDto.RepairOrderId);
            if (repairOrder == null)
                throw new ArgumentException("Repair order not found");

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
            return MapToDto(createdInspection);
        }

        public async Task<InspectionDto> UpdateInspectionAsync(Guid inspectionId, UpdateInspectionDto updateInspectionDto)
        {
            var inspection = await _inspectionRepository.GetByIdAsync(inspectionId);
            if (inspection == null)
                throw new ArgumentException("Inspection not found");

            // Update technician if provided
            if (updateInspectionDto.TechnicianId.HasValue)
            {
                inspection.TechnicianId = updateInspectionDto.TechnicianId.Value;
            }

            // Update other fields if provided
            if (!string.IsNullOrEmpty(updateInspectionDto.CustomerConcern))
                inspection.CustomerConcern = updateInspectionDto.CustomerConcern;
                
            if (updateInspectionDto.Finding != null)
                inspection.Finding = updateInspectionDto.Finding;
                
            inspection.IssueRating = updateInspectionDto.IssueRating;
            
            if (updateInspectionDto.Note != null)
                inspection.Note = updateInspectionDto.Note;
                
                
            inspection.UpdatedAt = DateTime.UtcNow;

            var updatedInspection = await _inspectionRepository.UpdateAsync(inspection);
            return MapToDto(updatedInspection);
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
            decimal inspectionFee = 0;
            bool hasAdvancedService = inspection.ServiceInspections
                .Any(si => si.Service != null && si.Service.IsAdvanced);

            // 1=Basic, 2=Advanced
            int inspectionTypeId = hasAdvancedService ? 2 : 1;
            var inspectionType = await _dbContext.InspectionTypes
                .FirstOrDefaultAsync(it => it.InspectionTypeId == inspectionTypeId && it.IsActive);

            if (inspectionType != null)
            {
                inspectionFee = inspectionType.InspectionFee;
            }

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


            if (inspectionFee > 0)
            {
                var quotationEntity = await _dbContext.Quotations
                    .Include(q => q.RepairOrder)
                    .FirstOrDefaultAsync(q => q.QuotationId == quotation.QuotationId);

                if (quotationEntity != null)
                {
                    quotationEntity.TotalAmount += inspectionFee;
                    
                    // Add ONLY inspection fee to RepairOrder cost now
                    // Service/part fees will be added when quotation is approved
                    if (quotationEntity.RepairOrder != null)
                    {
                        quotationEntity.RepairOrder.Cost = inspectionFee;
                    }

                    await _dbContext.SaveChangesAsync();
                    
                    quotation.TotalAmount += inspectionFee;
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
    }
}