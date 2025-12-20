using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using DataAccessLayer;
using Dtos.InspectionAndRepair;
using Microsoft.EntityFrameworkCore;
using Repositories.InspectionAndRepair;
using Services.InspectionAndRepair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Services;
using Microsoft.AspNetCore.SignalR;
using Services.Hubs;

public class InspectionTechnicianService : IInspectionTechnicianService
{
    private readonly IInspectionTechnicianRepository _repo;
    private readonly IMapper _mapper;
    private readonly IRepairOrderService _repairOrderService;
    private readonly IHubContext<InspectionHub> _inspectionHubContext;
    private readonly MyAppDbContext _context;

    public InspectionTechnicianService(
        IInspectionTechnicianRepository repo,
        IMapper mapper,
        IRepairOrderService repairOrderService,
        IHubContext<InspectionHub> inspectionHubContext,
        MyAppDbContext context)
    {
        _repo = repo;
        _mapper = mapper;
        _repairOrderService = repairOrderService;
        _inspectionHubContext = inspectionHubContext;
        _context = context;
    }
    public async Task<List<InspectionTechnicianDto>> GetInspectionsByTechnicianAsync(string userId)
    {
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null)
            return new List<InspectionTechnicianDto>();

        var inspections = await _repo.GetInspectionsByTechnicianIdAsync(technician.TechnicianId);
        var dtos = _mapper.Map<List<InspectionTechnicianDto>>(inspections);

        foreach (var dto in dtos)
        {
            var inspection = inspections.FirstOrDefault(i => i.InspectionId == dto.InspectionId);

            FilterPartCategoriesByModel(dto, inspection);
            AttachSuggestedParts(dto, inspection);
        }

        dtos = dtos
            .OrderBy(i => i.Status == InspectionStatus.New ? 0 :
                          i.Status == InspectionStatus.InProgress ? 1 :
                          i.Status == InspectionStatus.Completed ? 2 : 3)
            .ThenBy(i => i.RepairOrder?.Vehicle?.Brand.BrandName)
            .ToList();

        return dtos;
    }

    public async Task<InspectionTechnicianDto?> GetInspectionByIdAsync(Guid id, string userId)
    {
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null) return null;

        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(id, technician.TechnicianId);
        if (inspection == null) return null;

        var dto = _mapper.Map<InspectionTechnicianDto>(inspection);

        FilterPartCategoriesByModel(dto, inspection);
        AttachSuggestedParts(dto, inspection);
        return dto;
    }

    public async Task<InspectionTechnicianDto> StartInspectionAsync(Guid id, string userId)
    {
        // get technician and entity
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null) throw new InvalidOperationException("You do not have permission.");

        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(id, technician.TechnicianId);
        if (inspection == null) throw new InvalidOperationException("Inspection does not exist or you do not have permission.");

        if (inspection.Status != InspectionStatus.New)
            throw new InvalidOperationException("Inspection can only be started when status is 'New'");

        inspection.Status = InspectionStatus.InProgress;
        inspection.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync();

        // Send SignalR notification to managers
        var technicianName = technician.User != null
            ? $"{technician.User.FirstName} {technician.User.LastName}".Trim()
            : "Unknown Technician";

        await _inspectionHubContext.Clients
            .Group("Managers")
            .SendAsync("InspectionStarted", new
            {
                InspectionId = id,
                RepairOrderId = inspection.RepairOrderId,
                TechnicianId = technician.TechnicianId,
                TechnicianName = technicianName,
                CustomerConcern = inspection.CustomerConcern,
                StartedAt = DateTime.UtcNow,
                Message = "Technician has started the inspection"
            });

        Console.WriteLine($"[InspectionTechnicianService] Sent InspectionStarted to Managers group for Inspection {id}");

        var dto = _mapper.Map<InspectionTechnicianDto>(inspection);
        AttachSuggestedParts(dto, inspection);
        return dto;
    }
    public async Task<InspectionTechnicianDto> UpdateInspectionAsync(Guid id, UpdateInspectionRequest request, string userId)
    {
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null) throw new InvalidOperationException("You do not have permission.");

        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(id, technician.TechnicianId);
        if (inspection == null) throw new InvalidOperationException("Inspection not found or you do not have update permission.");

        // Store the previous status to check if inspection was just completed
        var previousStatus = inspection.Status;

        if (inspection.Status != InspectionStatus.New &&
            inspection.Status != InspectionStatus.Pending &&
            inspection.Status != InspectionStatus.InProgress)
        {
            throw new InvalidOperationException("Cannot update completed or locked inspection.");
        }

        bool canComplete = true;
        var validationErrors = new List<string>();

        foreach (var serviceUpdate in request.ServiceUpdates)
        {
            if (serviceUpdate.ConditionStatus == ConditionStatus.Needs_Attention ||
                serviceUpdate.ConditionStatus == ConditionStatus.Replace)
            {
                if (serviceUpdate.SuggestedPartsByCategory == null || !serviceUpdate.SuggestedPartsByCategory.Any())
                {
                    validationErrors.Add($"Service must have PartCategory and Part selected when status is {serviceUpdate.ConditionStatus}");
                    canComplete = false;
                }
                else
                {
                    foreach (var kvp in serviceUpdate.SuggestedPartsByCategory)
                    {
                        if (kvp.Value == null || !kvp.Value.Any())
                        {
                            validationErrors.Add($"PartCategory must have at least 1 Part");
                            canComplete = false;
                        }
                    }
                }
            }
        }

        if (string.IsNullOrWhiteSpace(request.Finding))
        {
            validationErrors.Add("Finding cannot be empty");
            canComplete = false;
        }

        if (request.IsCompleted && !canComplete)
        {
            throw new InvalidOperationException($"Cannot complete inspection: {string.Join("; ", validationErrors)}");
        }

        inspection.Finding = request.Finding;
        inspection.UpdatedAt = DateTime.UtcNow;

        var repairOrderServices = await _repo.GetRepairOrderServicesAsync(inspection.RepairOrderId);
        var hasRepairOrderServices = repairOrderServices.Any();

        var existingServiceInspections = inspection.ServiceInspections.ToDictionary(si => si.ServiceId, si => si);

        var allPartsToValidate = new Dictionary<Guid, List<PartWithQuantityDto>>();
        foreach (var serviceUpdate in request.ServiceUpdates)
        {
            if (serviceUpdate.ConditionStatus == ConditionStatus.Replace ||
                serviceUpdate.ConditionStatus == ConditionStatus.Needs_Attention)
            {
                if (serviceUpdate.SuggestedPartsByCategory != null && serviceUpdate.SuggestedPartsByCategory.Any())
                {
                    foreach (var kvp in serviceUpdate.SuggestedPartsByCategory)
                    {
                        allPartsToValidate[kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        if (allPartsToValidate.Any())
        {
            try
            {
                await ValidateAndReservePartStockAsync(id, allPartsToValidate);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Stock validation failed: {ex.Message}");
            }
        }

        foreach (var serviceUpdate in request.ServiceUpdates)
        {
            Service service;

            if (hasRepairOrderServices)
            {
                var repairOrderService = repairOrderServices.FirstOrDefault(ros => ros.ServiceId == serviceUpdate.ServiceId);
                if (repairOrderService == null)
                    throw new InvalidOperationException("Service does not exist in this RepairOrder.");

                service = repairOrderService.Service;
            }
            else
            {
                if (!existingServiceInspections.ContainsKey(serviceUpdate.ServiceId))
                    throw new InvalidOperationException("Service has not been added to Inspection.");

                service = existingServiceInspections[serviceUpdate.ServiceId].Service;
            }

            ServiceInspection serviceInspection;
            if (existingServiceInspections.ContainsKey(serviceUpdate.ServiceId))
            {
                serviceInspection = existingServiceInspections[serviceUpdate.ServiceId];
                serviceInspection.ConditionStatus = serviceUpdate.ConditionStatus;
            }
            else
            {
                serviceInspection = new ServiceInspection
                {
                    ServiceInspectionId = Guid.NewGuid(),
                    InspectionId = id,
                    ServiceId = serviceUpdate.ServiceId,
                    ConditionStatus = serviceUpdate.ConditionStatus,
                    CreatedAt = DateTime.UtcNow
                };
                _repo.AddServiceInspection(serviceInspection);
            }

            var vehicleModelId = inspection.RepairOrder?.Vehicle?.ModelId;
            if (!vehicleModelId.HasValue)
                throw new InvalidOperationException("Vehicle model information is missing.");


            var allowedPartCategoryIds = service.ServicePartCategories
                .Where(spc => spc.PartCategory.ModelId == vehicleModelId.Value)
                .Select(spc => spc.PartCategoryId)
                .ToHashSet();

            if (serviceUpdate.ConditionStatus == ConditionStatus.Good ||
                serviceUpdate.ConditionStatus == ConditionStatus.Not_Checked)
            {
                // Remove parts if any
                var partsToRemove = inspection.PartInspections
                    .Where(pi => allowedPartCategoryIds.Contains(pi.PartCategoryId))
                    .ToList();

                if (partsToRemove.Any())
                    _repo.RemovePartInspections(partsToRemove);
                continue;
            }

            if (serviceUpdate.ConditionStatus == ConditionStatus.Replace ||
                serviceUpdate.ConditionStatus == ConditionStatus.Needs_Attention)
            {
                if (serviceUpdate.SuggestedPartsByCategory == null || !serviceUpdate.SuggestedPartsByCategory.Any())
                {
                    var partsToRemove = inspection.PartInspections
                        .Where(pi => allowedPartCategoryIds.Contains(pi.PartCategoryId))
                        .ToList();

                    if (partsToRemove.Any())
                        _repo.RemovePartInspections(partsToRemove);

                    continue;
                }

                if (!allowedPartCategoryIds.Any())
                    throw new InvalidOperationException("Service has no PartCategory configured.");

                var selectedCategoryIds = serviceUpdate.SuggestedPartsByCategory.Keys.ToList();
                foreach (var categoryId in selectedCategoryIds)
                {
                    if (!allowedPartCategoryIds.Contains(categoryId))
                        throw new InvalidOperationException("PartCategory is not allowed for this service.");
                }

                if (!service.IsAdvanced)
                {
                    if (selectedCategoryIds.Count > 1)
                        throw new InvalidOperationException("Service only allows selecting parts from 1 PartCategory.");
                }

                foreach (var kvp in serviceUpdate.SuggestedPartsByCategory)
                {
                    var categoryId = kvp.Key;
                    var partRequests = kvp.Value;

                    if (partRequests == null || !partRequests.Any())
                        throw new InvalidOperationException("PartCategory must have at least 1 Part.");

                    var category = service.ServicePartCategories
                        .FirstOrDefault(spc => spc.PartCategoryId == categoryId);

                    if (category == null)
                        throw new InvalidOperationException("PartCategory does not exist in service.");

                    var validPartIdsInCategory = category.PartCategory.Parts
                        .Select(p => p.PartId)
                        .ToHashSet();

                    foreach (var partRequest in partRequests)
                    {
                        if (!validPartIdsInCategory.Contains(partRequest.PartId))
                            throw new InvalidOperationException("Part does not belong to the selected PartCategory.");

                        if (partRequest.Quantity <= 0)
                            throw new InvalidOperationException("Part quantity must be greater than 0.");
                    }
                }

                var existingParts = inspection.PartInspections
                    .Where(pi => allowedPartCategoryIds.Contains(pi.PartCategoryId))
                    .ToList();

                if (existingParts.Any())
                    _repo.RemovePartInspections(existingParts);

                foreach (var kvp in serviceUpdate.SuggestedPartsByCategory)
                {
                    var categoryId = kvp.Key;
                    var partRequests = kvp.Value;

                    foreach (var partRequest in partRequests)
                    {
                        _repo.AddPartInspection(new PartInspection
                        {
                            PartInspectionId = Guid.NewGuid(),
                            InspectionId = id,
                            PartId = partRequest.PartId,
                            PartCategoryId = categoryId,
                            Quantity = partRequest.Quantity,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }
        if (request.IsCompleted && canComplete)
        {
            inspection.Status = InspectionStatus.Completed;
        }
        else if (inspection.Status == InspectionStatus.New || inspection.Status == InspectionStatus.Pending)
        {
            inspection.Status = InspectionStatus.InProgress;
        }

        await _repo.SaveChangesAsync();

        // Send SignalR notification only when inspection completes
        if (previousStatus != InspectionStatus.Completed && inspection.Status == InspectionStatus.Completed)
        {
            var technicianName = technician.User != null
                ? $"{technician.User.FirstName} {technician.User.LastName}".Trim()
                : "Unknown Technician";

            await _inspectionHubContext.Clients
                .Group("Managers")
                .SendAsync("InspectionCompleted", new
                {
                    InspectionId = id,
                    RepairOrderId = inspection.RepairOrderId,
                    TechnicianId = technician.TechnicianId,
                    TechnicianName = technicianName,
                    CustomerConcern = inspection.CustomerConcern,
                    Finding = inspection.Finding,
                    ServiceCount = inspection.ServiceInspections?.Count ?? 0,
                    PartCount = inspection.PartInspections?.Count ?? 0,
                    CompletedAt = DateTime.UtcNow,
                    Message = "Inspection completed and ready for quotation"
                });

            Console.WriteLine($"[InspectionTechnicianService] Sent InspectionCompleted to Managers group for Inspection {id}");
        }


        var dto = _mapper.Map<InspectionTechnicianDto>(inspection);
        AttachSuggestedParts(dto, inspection);
        return dto;
    }

    public async Task<bool> RemovePartFromInspectionAsync(Guid inspectionId, Guid serviceId, Guid partInspectionId, string userId)
    {
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null)
            throw new InvalidOperationException("You do not have permission.");

        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(inspectionId, technician.TechnicianId);
        if (inspection == null)
            throw new InvalidOperationException("Inspection not found or you do not have permission.");

        if (inspection.Status == InspectionStatus.Completed)
            throw new InvalidOperationException("Cannot remove part from completed inspection.");
        if (inspection.Status != InspectionStatus.Pending && inspection.Status != InspectionStatus.InProgress)
            throw new InvalidOperationException("Parts can only be removed when inspection is Pending or InProgress.");

        var serviceInspection = inspection.ServiceInspections.FirstOrDefault(si => si.ServiceId == serviceId);
        if (serviceInspection == null)
            throw new InvalidOperationException("Service not found in inspection.");

        var partInspection = inspection.PartInspections.FirstOrDefault(pi => pi.PartInspectionId == partInspectionId);
        if (partInspection == null)
            throw new InvalidOperationException("Part not found in inspection.");

        var validPartCategoryIds = serviceInspection.Service.ServicePartCategories
            .Select(spc => spc.PartCategoryId)
            .ToHashSet();

        if (!validPartCategoryIds.Contains(partInspection.PartCategoryId))
            throw new InvalidOperationException("Part does not belong to the service's PartCategory.");

        _repo.RemovePartInspections(new List<PartInspection> { partInspection });
        await _repo.SaveChangesAsync();

        return true;
    }
    public async Task<InspectionTechnicianDto> RemovePartCategoryFromServiceAsync(
    Guid inspectionId,
    Guid serviceInspectionId,
    Guid partCategoryId,
    string userId)
    {
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null)
            throw new InvalidOperationException("You do not have permission.");

        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(inspectionId, technician.TechnicianId);
        if (inspection == null)
            throw new InvalidOperationException("Inspection not found or you do not have permission.");

        // Check inspection status
        if (inspection.Status == InspectionStatus.Completed)
            throw new InvalidOperationException("Cannot remove PartCategory from completed inspection.");

        if (inspection.Status != InspectionStatus.Pending &&
            inspection.Status != InspectionStatus.InProgress &&
            inspection.Status != InspectionStatus.New)
            throw new InvalidOperationException("PartCategory can only be removed when inspection is New, Pending or InProgress.");

        var serviceInspection = inspection.ServiceInspections
            .FirstOrDefault(si => si.ServiceInspectionId == serviceInspectionId);
        if (serviceInspection == null)
            throw new InvalidOperationException("Service not found in inspection.");

        var servicePartCategory = serviceInspection.Service.ServicePartCategories
            .FirstOrDefault(spc => spc.PartCategoryId == partCategoryId);
        if (servicePartCategory == null)
            throw new InvalidOperationException("PartCategory does not belong to this service.");

        if (!serviceInspection.Service.IsAdvanced)
        {
            var partsInThisCategory = inspection.PartInspections
                .Where(pi => pi.PartCategoryId == partCategoryId)
                .ToList();

            if (partsInThisCategory.Any())
            {
                var otherCategories = serviceInspection.Service.ServicePartCategories
                    .Where(spc => spc.PartCategoryId != partCategoryId)
                    .ToList();

                if (!otherCategories.Any())
                    throw new InvalidOperationException(
                        "Cannot remove the last PartCategory when parts have been suggested. " +
                        "Please remove parts first or add another PartCategory.");
            }
        }

        var relatedPartInspections = inspection.PartInspections
            .Where(pi => pi.PartCategoryId == partCategoryId)
            .ToList();

        if (relatedPartInspections.Any())
        {
            _repo.RemovePartInspections(relatedPartInspections);
        }

        inspection.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync();

        inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(inspectionId, technician.TechnicianId);
        var dto = _mapper.Map<InspectionTechnicianDto>(inspection);
        AttachSuggestedParts(dto, inspection);

        return dto;
    }

    private void AttachSuggestedParts(InspectionTechnicianDto dto, Inspection? entity)
    {
        if (dto == null || entity == null) return;
        if (dto.ServiceInspections == null || dto.ServiceInspections.Count == 0) return;

        var vehicleModelId = entity.RepairOrder?.Vehicle?.ModelId;
        if (vehicleModelId == null) return;

        var partInsByPartId = (entity.PartInspections ?? Enumerable.Empty<PartInspection>())
            .GroupBy(pi => pi.PartId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var sDto in dto.ServiceInspections)
        {
            sDto.SuggestedParts = new List<PartInspectionDto>();

            var sEntity = entity.ServiceInspections?.FirstOrDefault(si => si.ServiceInspectionId == sDto.ServiceInspectionId);
            if (sEntity?.Service == null) continue;

            var allowedPartIds = sEntity.Service.ServicePartCategories?
                .Where(spc => spc.PartCategory.ModelId == vehicleModelId.Value)
                .SelectMany(spc => spc.PartCategory.Parts.Select(p => p.PartId))
                .ToHashSet() ?? new HashSet<Guid>();

            foreach (var pid in allowedPartIds)
            {
                if (partInsByPartId.TryGetValue(pid, out var pis))
                {
                    foreach (var pi in pis)
                    {
                        sDto.SuggestedParts.Add(_mapper.Map<PartInspectionDto>(pi));
                    }
                }
            }
        }
    }
    public async Task<List<AllServiceDto>> GetAllServicesAsync()
    {
        var services = await _repo.GetAllServicesAsync();
        return _mapper.Map<List<AllServiceDto>>(services);
    }
    public async Task<InspectionTechnicianDto> AddServiceToInspectionAsync(Guid inspectionId, AddServiceToInspectionRequest request, string userId)
    {
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null)
            throw new InvalidOperationException("You do not have permission.");

        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(inspectionId, technician.TechnicianId);
        if (inspection == null)
            throw new InvalidOperationException("Inspection not found or you do not have permission.");

        if (inspection.Status == InspectionStatus.Completed)
            throw new InvalidOperationException("Cannot add service to completed inspection.");

        var hasRepairOrderServices = await _repo.HasRepairOrderServicesAsync(inspection.RepairOrderId);
        if (hasRepairOrderServices)
            throw new InvalidOperationException("Cannot add service because this RepairOrder already has assigned services.");

        var service = await _repo.GetServiceByIdAsync(request.ServiceId);
        if (service == null)
            throw new InvalidOperationException("Service does not exist.");

        var existingServiceInspection = inspection.ServiceInspections
            .FirstOrDefault(si => si.ServiceId == request.ServiceId);
        if (existingServiceInspection != null)
            throw new InvalidOperationException("Service has already been added to inspection.");

        var serviceInspection = new ServiceInspection
        {
            ServiceInspectionId = Guid.NewGuid(),
            InspectionId = inspectionId,
            ServiceId = request.ServiceId,
            ConditionStatus = ConditionStatus.Not_Checked,
            CreatedAt = DateTime.UtcNow
        };

        _repo.AddServiceInspection(serviceInspection);

        if (inspection.Status == InspectionStatus.Pending)
        {
            inspection.Status = InspectionStatus.InProgress;
            inspection.UpdatedAt = DateTime.UtcNow;
        }

        await _repo.SaveChangesAsync();

        inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(inspectionId, technician.TechnicianId);
        var dto = _mapper.Map<InspectionTechnicianDto>(inspection);
        AttachSuggestedParts(dto, inspection);
        return dto;
    }

    public async Task<InspectionTechnicianDto> RemoveServiceFromInspectionAsync(Guid inspectionId, Guid serviceInspectionId, string userId)
    {
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null)
            throw new InvalidOperationException("You do not have permission.");

        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(inspectionId, technician.TechnicianId);
        if (inspection == null)
            throw new InvalidOperationException("Inspection not found or you do not have permission.");

        if (inspection.Status == InspectionStatus.Completed)
            throw new InvalidOperationException("Cannot remove service from completed inspection.");

        var hasRepairOrderServices = await _repo.HasRepairOrderServicesAsync(inspection.RepairOrderId);
        if (hasRepairOrderServices)
            throw new InvalidOperationException("Cannot remove service because this RepairOrder already has assigned services.");

        var serviceInspection = inspection.ServiceInspections
            .FirstOrDefault(si => si.ServiceInspectionId == serviceInspectionId);
        if (serviceInspection == null)
            throw new InvalidOperationException("Service not found in inspection.");

        var servicePartCategoryIds = serviceInspection.Service.ServicePartCategories
            .Select(spc => spc.PartCategoryId)
            .ToHashSet();

        var relatedPartInspections = inspection.PartInspections
            .Where(pi => servicePartCategoryIds.Contains(pi.PartCategoryId))
            .ToList();

        if (relatedPartInspections.Any())
            _repo.RemovePartInspections(relatedPartInspections);

        _repo.RemoveServiceInspection(serviceInspection);

        await _repo.SaveChangesAsync();

        inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(inspectionId, technician.TechnicianId);
        var dto = _mapper.Map<InspectionTechnicianDto>(inspection);
        AttachSuggestedParts(dto, inspection);
        return dto;
    }
    public async Task<TechnicianDto?> GetTechnicianByUserIdAsync(string userId)
    {
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null) return null;

        return new TechnicianDto
        {
            TechnicianId = technician.TechnicianId
        };
    }
    private void FilterPartCategoriesByModel(InspectionTechnicianDto dto, Inspection? entity)
    {
        if (dto == null || entity == null) return;

        var vehicleModelId = entity.RepairOrder?.Vehicle?.ModelId;
        if (!vehicleModelId.HasValue) return;

        foreach (var serviceDto in dto.ServiceInspections)
        {
            if (serviceDto.AllPartCategories != null && serviceDto.AllPartCategories.Any())
            {
                serviceDto.AllPartCategories = serviceDto.AllPartCategories
                    .Where(pc => pc.ModelId == vehicleModelId.Value) 
                    .ToList();
            }
        }
    }
    private async Task ValidateAndReservePartStockAsync(
        Guid inspectionId,
        Dictionary<Guid, List<PartWithQuantityDto>> partsByCategory)
    {
        var inspection = await _repo.GetInspectionWithRepairOrderAsync(inspectionId);
        if (inspection?.RepairOrder?.BranchId == null)
            throw new InvalidOperationException("Cannot find BranchId from RepairOrder.");

        var branchId = inspection.RepairOrder.BranchId;

        foreach (var kvp in partsByCategory)
        {
            var partRequests = kvp.Value;

            foreach (var partRequest in partRequests)
            {
                var partInventory = await _repo.GetPartInventoryAsync(partRequest.PartId, branchId);

                if (partInventory == null)
                {
                    var part = await _context.Parts.FindAsync(partRequest.PartId);
                    var partName = part?.Name ?? "Unknown Part";
                    throw new InvalidOperationException(
                        $"Currently '{partName}' is not available in stock");
                }

                if (partInventory.Stock < partRequest.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Currently '{partInventory.Part.Name}' only has {partInventory.Stock} available");
                }

                // Trừ stock
                partInventory.Stock -= partRequest.Quantity;
                partInventory.UpdatedAt = DateTime.UtcNow;
                _repo.UpdatePartInventory(partInventory);
            }
        }
    }
}