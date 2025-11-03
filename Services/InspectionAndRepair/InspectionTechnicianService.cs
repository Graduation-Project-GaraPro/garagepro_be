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

public class InspectionTechnicianService : IInspectionTechnicianService
{
    private readonly IInspectionTechnicianRepository _repo;
    private readonly IMapper _mapper;

    public InspectionTechnicianService(IInspectionTechnicianRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<InspectionTechnicianDto>> GetInspectionsByTechnicianAsync(string userId)
    {
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null) return new List<InspectionTechnicianDto>();

        var inspections = await _repo.GetInspectionsByTechnicianIdAsync(technician.TechnicianId);
        var dtos = _mapper.Map<List<InspectionTechnicianDto>>(inspections);

        foreach (var dto in dtos)
            AttachSuggestedParts(dto, inspections.FirstOrDefault(i => i.InspectionId == dto.InspectionId));

        return dtos;
    }

    public async Task<InspectionTechnicianDto?> GetInspectionByIdAsync(Guid id, string userId)
    {
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null) return null;

        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(id, technician.TechnicianId);
        if (inspection == null) return null;

        var dto = _mapper.Map<InspectionTechnicianDto>(inspection);
        AttachSuggestedParts(dto, inspection);
        return dto;
    }

    public async Task<InspectionTechnicianDto> StartInspectionAsync(Guid id, string userId)
    {
        // get technician and entity
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null) throw new InvalidOperationException("Bạn không có quyền.");

        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(id, technician.TechnicianId);
        if (inspection == null) throw new InvalidOperationException("Inspection không tồn tại hoặc bạn không có quyền.");

        if (inspection.Status != InspectionStatus.New)
            throw new InvalidOperationException("Chỉ có thể bắt đầu khi Inspection ở trạng thái 'New'.");

        inspection.Status = InspectionStatus.InProgress;
        inspection.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync();

        var dto = _mapper.Map<InspectionTechnicianDto>(inspection);
        AttachSuggestedParts(dto, inspection);
        return dto;
    }
    public async Task<InspectionTechnicianDto> UpdateInspectionAsync(Guid id, UpdateInspectionRequest request, string userId)
    {
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null) throw new InvalidOperationException("Bạn không có quyền.");

        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(id, technician.TechnicianId);
        if (inspection == null) throw new InvalidOperationException("Không tìm thấy Inspection hoặc bạn không có quyền cập nhật.");

        if (inspection.Status != InspectionStatus.New &&
            inspection.Status != InspectionStatus.Pending &&
            inspection.Status != InspectionStatus.InProgress)
        {
            throw new InvalidOperationException("Không thể cập nhật Inspection đã hoàn thành hoặc bị khóa.");
        }

        inspection.Finding = request.Finding;
        inspection.UpdatedAt = DateTime.UtcNow;

        var repairOrderServices = await _repo.GetRepairOrderServicesAsync(inspection.RepairOrderId);
        var hasRepairOrderServices = repairOrderServices.Any();

        var existingServiceInspections = inspection.ServiceInspections.ToDictionary(si => si.ServiceId, si => si);

        foreach (var serviceUpdate in request.ServiceUpdates)
        {
            Service service;

            if (hasRepairOrderServices)
            {
                var repairOrderService = repairOrderServices.FirstOrDefault(ros => ros.ServiceId == serviceUpdate.ServiceId);
                if (repairOrderService == null)
                    throw new InvalidOperationException($"ServiceId {serviceUpdate.ServiceId} không tồn tại trong RepairOrder này.");

                service = repairOrderService.Service;
            }
            else
            {
                if (!existingServiceInspections.ContainsKey(serviceUpdate.ServiceId))
                    throw new InvalidOperationException($"ServiceId {serviceUpdate.ServiceId} chưa được thêm vào Inspection.");

                service = existingServiceInspections[serviceUpdate.ServiceId].Service;
            }

            // Update hoặc tạo ServiceInspection
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

            var allowedPartCategoryIds = service.ServicePartCategories
                .Select(spc => spc.PartCategoryId)
                .ToHashSet();
            if (serviceUpdate.ConditionStatus == ConditionStatus.Good ||
                serviceUpdate.ConditionStatus == ConditionStatus.Not_Checked)
            {
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
                    throw new InvalidOperationException($"Dịch vụ {service.ServiceName} chưa được cấu hình PartCategory nào.");

                var selectedCategoryIds = serviceUpdate.SuggestedPartsByCategory.Keys.ToList();
                foreach (var categoryId in selectedCategoryIds)
                {
                    if (!allowedPartCategoryIds.Contains(categoryId))
                        throw new InvalidOperationException($"PartCategory {categoryId} không được phép cho dịch vụ {service.ServiceName}.");
                }

                if (!service.IsAdvanced)
                {
                    if (selectedCategoryIds.Count > 1)
                        throw new InvalidOperationException($"Dịch vụ {service.ServiceName} chỉ cho phép chọn parts từ 1 PartCategory.");
                }

                foreach (var kvp in serviceUpdate.SuggestedPartsByCategory)
                {
                    var categoryId = kvp.Key;
                    var partIds = kvp.Value;

                    if (partIds == null || !partIds.Any())
                        continue;

                    var category = service.ServicePartCategories
                        .FirstOrDefault(spc => spc.PartCategoryId == categoryId);

                    if (category == null)
                        throw new InvalidOperationException($"PartCategory {categoryId} không tồn tại trong service.");

                    var validPartIdsInCategory = category.PartCategory.Parts
                        .Select(p => p.PartId)
                        .ToHashSet();

                    foreach (var partId in partIds)
                    {
                        if (!validPartIdsInCategory.Contains(partId))
                            throw new InvalidOperationException($"Part {partId} không thuộc PartCategory {category.PartCategory.CategoryName}.");
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
                    var partIds = kvp.Value;

                    if (partIds == null || !partIds.Any())
                        continue;

                    foreach (var partId in partIds)
                    {
                        _repo.AddPartInspection(new PartInspection
                        {
                            PartInspectionId = Guid.NewGuid(),
                            InspectionId = id,
                            PartId = partId,
                            PartCategoryId = categoryId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        bool allServicesCompleted = request.ServiceUpdates.All(su =>
        su.ConditionStatus != null &&
        su.SuggestedPartsByCategory != null && su.SuggestedPartsByCategory.Any() &&
        su.SuggestedPartsByCategory.All(spc => spc.Value != null && spc.Value.Any())
    ) && !string.IsNullOrWhiteSpace(request.Finding);

        inspection.Status = allServicesCompleted
            ? InspectionStatus.Completed
            : InspectionStatus.InProgress;

        await _repo.SaveChangesAsync();

        var dto = _mapper.Map<InspectionTechnicianDto>(inspection);
        AttachSuggestedParts(dto, inspection);
        return dto;
    }

    public async Task<bool> RemovePartFromInspectionAsync(Guid inspectionId, Guid serviceId, Guid partInspectionId, string userId)
    {
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null)
            throw new InvalidOperationException("Bạn không có quyền.");

        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(inspectionId, technician.TechnicianId);
        if (inspection == null)
            throw new InvalidOperationException("Không tìm thấy Inspection hoặc bạn không có quyền.");

        if (inspection.Status == InspectionStatus.Completed)
            throw new InvalidOperationException("Không thể xóa phụ tùng trong Inspection đã hoàn thành.");
        if (inspection.Status != InspectionStatus.Pending && inspection.Status != InspectionStatus.InProgress)
            throw new InvalidOperationException("Chỉ có thể xóa phụ tùng khi Inspection đang Pending hoặc InProgress.");

        var serviceInspection = inspection.ServiceInspections.FirstOrDefault(si => si.ServiceId == serviceId);
        if (serviceInspection == null)
            throw new InvalidOperationException("Không tìm thấy dịch vụ này trong Inspection.");

        var partInspection = inspection.PartInspections.FirstOrDefault(pi => pi.PartInspectionId == partInspectionId);
        if (partInspection == null)
            throw new InvalidOperationException("Không tìm thấy phụ tùng trong Inspection.");

        //  Kiểm tra part có thuộc PartCategory của service đó không
        var validPartCategoryIds = serviceInspection.Service.ServicePartCategories
            .Select(spc => spc.PartCategoryId)
            .ToHashSet();

        if (!validPartCategoryIds.Contains(partInspection.PartCategoryId))
            throw new InvalidOperationException("Phụ tùng không thuộc PartCategory của dịch vụ được chọn.");

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
        // Kiểm tra quyền technician
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null)
            throw new InvalidOperationException("Bạn không có quyền.");

        // Lấy Inspection
        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(inspectionId, technician.TechnicianId);
        if (inspection == null)
            throw new InvalidOperationException("Không tìm thấy Inspection hoặc bạn không có quyền.");

        // Kiểm tra trạng thái Inspection
        if (inspection.Status == InspectionStatus.Completed)
            throw new InvalidOperationException("Không thể xóa PartCategory trong Inspection đã hoàn thành.");

        if (inspection.Status != InspectionStatus.Pending &&
            inspection.Status != InspectionStatus.InProgress &&
            inspection.Status != InspectionStatus.New)
            throw new InvalidOperationException("Chỉ có thể xóa PartCategory khi Inspection đang New, Pending hoặc InProgress.");

        var hasRepairOrderServices = await _repo.HasRepairOrderServicesAsync(inspection.RepairOrderId);
        if (hasRepairOrderServices)
            throw new InvalidOperationException("Không thể xóa PartCategory vì RepairOrder này đã có service được chỉ định trong RepairOrderService.");

        var serviceInspection = inspection.ServiceInspections
            .FirstOrDefault(si => si.ServiceInspectionId == serviceInspectionId);
        if (serviceInspection == null)
            throw new InvalidOperationException("Không tìm thấy service trong Inspection.");

        var servicePartCategory = serviceInspection.Service.ServicePartCategories
            .FirstOrDefault(spc => spc.PartCategoryId == partCategoryId);
        if (servicePartCategory == null)
            throw new InvalidOperationException("PartCategory không thuộc service này.");


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
                        "Không thể xóa PartCategory cuối cùng khi đã có parts được gợi ý. " +
                        "Vui lòng xóa parts trước hoặc thêm PartCategory khác.");
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

        var partInsByPartId = (entity.PartInspections ?? Enumerable.Empty<PartInspection>())
            .GroupBy(pi => pi.PartId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var sDto in dto.ServiceInspections)
        {
            sDto.SuggestedParts = new List<PartInspectionDto>();

            var sEntity = entity.ServiceInspections?.FirstOrDefault(si => si.ServiceInspectionId == sDto.ServiceInspectionId);
            if (sEntity?.Service == null) continue;

            var allowedPartIds = sEntity.Service.ServicePartCategories?
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
            throw new InvalidOperationException("Bạn không có quyền.");

        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(inspectionId, technician.TechnicianId);
        if (inspection == null)
            throw new InvalidOperationException("Không tìm thấy Inspection hoặc bạn không có quyền.");

        if (inspection.Status == InspectionStatus.Completed)
            throw new InvalidOperationException("Không thể thêm service vào Inspection đã hoàn thành.");

        var hasRepairOrderServices = await _repo.HasRepairOrderServicesAsync(inspection.RepairOrderId);
        if (hasRepairOrderServices)
            throw new InvalidOperationException("Không thể thêm service vì RepairOrder này đã có service được chỉ định.");

        var service = await _repo.GetServiceByIdAsync(request.ServiceId);
        if (service == null)
            throw new InvalidOperationException("Service không tồn tại.");

        var existingServiceInspection = inspection.ServiceInspections
            .FirstOrDefault(si => si.ServiceId == request.ServiceId);
        if (existingServiceInspection != null)
            throw new InvalidOperationException("Service này đã được thêm vào Inspection.");

        var serviceInspection = new ServiceInspection
        {
            ServiceInspectionId = Guid.NewGuid(),
            InspectionId = inspectionId,
            ServiceId = request.ServiceId,
            ConditionStatus = ConditionStatus.Not_Checked,
            CreatedAt = DateTime.UtcNow
        };

        _repo.AddServiceInspection(serviceInspection);

        if (inspection.Status == InspectionStatus.New)
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
            throw new InvalidOperationException("Bạn không có quyền.");

        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(inspectionId, technician.TechnicianId);
        if (inspection == null)
            throw new InvalidOperationException("Không tìm thấy Inspection hoặc bạn không có quyền.");

        if (inspection.Status == InspectionStatus.Completed)
            throw new InvalidOperationException("Không thể xóa service trong Inspection đã hoàn thành.");

        var hasRepairOrderServices = await _repo.HasRepairOrderServicesAsync(inspection.RepairOrderId);
        if (hasRepairOrderServices)
            throw new InvalidOperationException("Không thể xóa service vì RepairOrder này đã có service được chỉ định.");

        var serviceInspection = inspection.ServiceInspections
            .FirstOrDefault(si => si.ServiceInspectionId == serviceInspectionId);
        if (serviceInspection == null)
            throw new InvalidOperationException("Không tìm thấy service trong Inspection.");

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
}
