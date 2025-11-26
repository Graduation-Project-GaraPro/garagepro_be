﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
using Dtos.Quotations;
using Microsoft.AspNetCore.SignalR;
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
        public InspectionService(
            IInspectionRepository inspectionRepository,
            IRepairOrderRepository repairOrderRepository,
            IQuotationService quotationService,
            IHubContext<TechnicianAssignmentHub> technicianAssignmentHubContext,
            IHubContext<InspectionHub> inspectionHubContext,
            INotificationService notificationService)
        {
            _inspectionRepository = inspectionRepository;
            _repairOrderRepository = repairOrderRepository;
            _quotationService = quotationService;
            _technicianAssignmentHubContext = technicianAssignmentHubContext;
            _inspectionHubContext = inspectionHubContext;
            _notificationService = notificationService;
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
            var technicianName = technician?.User?.FullName ?? "Unknown Technician";

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
            // Get the completed inspection with details
            var inspection = await _inspectionRepository.GetByIdAsync(convertDto.InspectionId);
            if (inspection == null)
                throw new ArgumentException("Inspection not found");

            // Check if inspection is completed
            if (inspection.Status != InspectionStatus.Completed)
                throw new InvalidOperationException("Only completed inspections can be converted to quotations");

            // Validate required relationships
            if (inspection.RepairOrder == null)
                throw new InvalidOperationException("Inspection must have a valid repair order");

            // Create quotation services from inspection services and parts
            var quotationServices = new List<CreateQuotationServiceDto>();
            
            // Add services from the inspection
            if (inspection.ServiceInspections != null)
            {
                foreach (var serviceInspection in inspection.ServiceInspections)
                {
                    // Skip if service is null
                    if (serviceInspection.Service == null)
                        continue;

                    var quotationService = new CreateQuotationServiceDto
                    {
                        ServiceId = serviceInspection.ServiceId,
                        IsSelected = true,
                        IsRequired = true,
                        QuotationServiceParts = new List<CreateQuotationServicePartDto>()
                    };
                    
                    // Add parts for this service
                    //var servicePartIds = serviceInspection.Service.ServiceParts?.Select(sp => sp.PartId).ToList() ?? new List<Guid>();
                    var servicePartIds = serviceInspection.Service.ServicePartCategories?.Select(sp => sp.PartCategoryId).ToList() ?? new List<Guid>();
                    var partInspections = inspection.PartInspections?.Where(pi => servicePartIds.Contains(pi.PartId)).ToList() ?? new List<PartInspection>();
                    
                    foreach (var partInspection in partInspections)
                    {
                        quotationService.QuotationServiceParts.Add(new CreateQuotationServicePartDto
                        {
                            PartId = partInspection.PartId,
                            IsSelected = true,
                            Quantity = 1
                        });
                    }
                    
                    quotationServices.Add(quotationService);
                }
            }

            // Create the quotation DTO
            var createQuotationDto = new CreateQuotationDto
            {
                InspectionId = inspection.InspectionId,
                RepairOrderId = inspection.RepairOrderId,
                UserId = inspection.RepairOrder.UserId,
                VehicleId = inspection.RepairOrder.VehicleId,
                Note = convertDto.Note,
                QuotationServices = quotationServices
            };

            // Create the quotation
            return await _quotationService.CreateQuotationAsync(createQuotationDto);
        }

        private InspectionDto MapToDto(Inspection inspection)
        {
            //return new InspectionDto
            //{
            //    InspectionId = inspection.InspectionId,
            //    RepairOrderId = inspection.RepairOrderId,
            //    TechnicianId = inspection.TechnicianId,
            //    Status = inspection.Status.ToString(),
            //    CustomerConcern = inspection.CustomerConcern,
            //    Finding = inspection.Finding,
            //    IssueRating = inspection.IssueRating,
            //    Note = inspection.Note,
            //    CreatedAt = inspection.CreatedAt,
            //    UpdatedAt = inspection.UpdatedAt,
            //    TechnicianName = inspection.Technician?.User?.FullName ?? "Unknown Technician",
            //    Services = inspection.ServiceInspections?.Select(si => new InspectionServiceDto
            //    {
            //        ServiceInspectionId = si.ServiceInspectionId,
            //        ServiceId = si.ServiceId,
            //        ServiceName = si.Service?.ServiceName ?? "Unknown Service",
            //        ConditionStatus = si.ConditionStatus,
            //        CreatedAt = si.CreatedAt,
            //        Parts = inspection.PartInspections?
            //            .Where(pi => si.Service?.ServiceParts?.Any(sp => sp.PartId == pi.PartId) == true)
            //            .Select(pi => new InspectionPartDto
            //            {
            //                PartInspectionId = pi.PartInspectionId,
            //                PartId = pi.PartId,
            //                PartName = pi.Part?.Name ?? "Unknown Part",
            //                CreatedAt = pi.CreatedAt
            //            }).ToList() ?? new List<InspectionPartDto>()
            //    }).ToList() ?? new List<InspectionServiceDto>()
            //};
            return new InspectionDto();
        }

        private CompletedInspectionDto MapToCompletedInspectionDto(Inspection inspection)
        {
            //return new CompletedInspectionDto
            //{
            //    InspectionId = inspection.InspectionId,
            //    RepairOrderId = inspection.RepairOrderId,
            //    TechnicianId = inspection.TechnicianId,
            //    Status = inspection.Status.ToString(),
            //    CustomerConcern = inspection.CustomerConcern,
            //    Finding = inspection.Finding,
            //    IssueRating = inspection.IssueRating,
            //    Note = inspection.Note,
            //    CreatedAt = inspection.CreatedAt,
            //    UpdatedAt = inspection.UpdatedAt,
            //    TechnicianName = inspection.Technician?.User?.FullName ?? "Unknown Technician",
            //    Services = inspection.ServiceInspections?.Select(si => new InspectionServiceDto
            //    {
            //        ServiceInspectionId = si.ServiceInspectionId,
            //        ServiceId = si.ServiceId,
            //        ServiceName = si.Service?.ServiceName ?? "Unknown Service",
            //        ConditionStatus = si.ConditionStatus,
            //        CreatedAt = si.CreatedAt,
            //        Parts = inspection.PartInspections?
            //            .Where(pi => si.Service?.ServiceParts?.Any(sp => sp.PartId == pi.PartId) == true)
            //            .Select(pi => new InspectionPartDto
            //            {
            //                PartInspectionId = pi.PartInspectionId,
            //                PartId = pi.PartId,
            //                PartName = pi.Part?.Name ?? "Unknown Part",
            //                CreatedAt = pi.CreatedAt
            //            }).ToList() ?? new List<InspectionPartDto>()
            //    }).ToList() ?? new List<InspectionServiceDto>()
            //};
            return new CompletedInspectionDto();
        }
    }
}