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

        // attach suggested parts per service inspection
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
        if (!repairOrderServices.Any())
            throw new InvalidOperationException("RepairOrder không có dịch vụ nào để kiểm tra.");

        var existingServiceInspections = inspection.ServiceInspections.ToDictionary(si => si.ServiceId, si => si);

        foreach (var serviceUpdate in request.ServiceUpdates)
        {
            var repairOrderService = repairOrderServices.FirstOrDefault(ros => ros.ServiceId == serviceUpdate.ServiceId);
            if (repairOrderService == null)
                throw new InvalidOperationException($"ServiceId {serviceUpdate.ServiceId} không tồn tại trong RepairOrder này.");

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

            // Xử lý Suggested Parts theo logic IsAdvanced
            if (serviceUpdate.SuggestedPartIds != null && serviceUpdate.SuggestedPartIds.Any())
            {
                if (serviceUpdate.ConditionStatus != ConditionStatus.Replace &&
                    serviceUpdate.ConditionStatus != ConditionStatus.Needs_Attention)
                    throw new InvalidOperationException("Chỉ có thể gợi ý phụ tùng khi tình trạng là Replace hoặc Needs_Attention.");

                var service = repairOrderService.Service;
                var servicePartIds = service.ServiceParts.Select(sp => sp.PartId).ToList();

                // Kiểm tra Part hợp lệ
                foreach (var partId in serviceUpdate.SuggestedPartIds)
                {
                    if (!servicePartIds.Contains(partId))
                        throw new InvalidOperationException($"Phụ tùng ID {partId} không hợp lệ cho dịch vụ {service.ServiceName}.");
                }

                // Lấy danh sách PartInspection hiện có
                var oldPartInspections = inspection.PartInspections
                    .Where(pi => servicePartIds.Contains(pi.PartId))
                    .ToList();

                // Nếu Service.IsAdvanced == false → chỉ được chọn 1 part
                if (!service.IsAdvanced)
                {
                    if (serviceUpdate.SuggestedPartIds.Count > 1)
                        throw new InvalidOperationException($"Dịch vụ {service.ServiceName} chỉ cho phép chọn 1 phụ tùng.");

                    // Xóa part cũ trước đó, vì chỉ được có 1
                    if (oldPartInspections.Any())
                        _repo.RemovePartInspections(oldPartInspections);

                    // Thêm part mới duy nhất
                    _repo.AddPartInspection(new PartInspection
                    {
                        PartInspectionId = Guid.NewGuid(),
                        InspectionId = id,
                        PartId = serviceUpdate.SuggestedPartIds.First(),
                        Status = "Suggested",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    // Service.IsAdvanced == true → cho phép nhiều part
                    // Lấy danh sách part đã gợi ý trước đó
                    var existingPartIds = oldPartInspections.Select(pi => pi.PartId).ToHashSet();

                    // Lọc ra các part mới chưa được suggest
                    var newPartIds = serviceUpdate.SuggestedPartIds
                        .Where(partId => !existingPartIds.Contains(partId))
                        .ToList();

                    foreach (var partId in newPartIds)
                    {
                        _repo.AddPartInspection(new PartInspection
                        {
                            PartInspectionId = Guid.NewGuid(),
                            InspectionId = id,
                            PartId = partId,
                            Status = "Suggested",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    var duplicatePartIds = serviceUpdate.SuggestedPartIds
                        .Where(partId => existingPartIds.Contains(partId))
                        .ToList();

                    if (duplicatePartIds.Any())
                    {
                        Console.WriteLine($"Các part đã tồn tại: {string.Join(", ", duplicatePartIds)}");
                    }
                }
            }
        }
        if (request.IsCompleted)
            inspection.Status = InspectionStatus.Pending;
        else if (inspection.Status == InspectionStatus.New)
            inspection.Status = InspectionStatus.InProgress;

        await _repo.SaveChangesAsync();

        var dto = _mapper.Map<InspectionTechnicianDto>(inspection);
        AttachSuggestedParts(dto, inspection);
        return dto;
    }

    public async Task<bool> RemovePartFromInspectionAsync(Guid inspectionId, Guid serviceId, Guid partInspectionId, string userId)
    {
        // Kiểm tra kỹ thuật viên có hợp lệ không
        var technician = await _repo.GetTechnicianByUserIdAsync(userId);
        if (technician == null)
            throw new InvalidOperationException("Bạn không có quyền.");

        // Lấy Inspection có liên quan đến technician
        var inspection = await _repo.GetInspectionByIdAndTechnicianIdAsync(inspectionId, technician.TechnicianId);
        if (inspection == null)
            throw new InvalidOperationException("Không tìm thấy Inspection hoặc bạn không có quyền.");

        // Kiểm tra trạng thái Inspection
        if (inspection.Status == InspectionStatus.Completed)
            throw new InvalidOperationException("Không thể xóa phụ tùng trong Inspection đã hoàn thành.");
        if (inspection.Status != InspectionStatus.Pending && inspection.Status != InspectionStatus.InProgress)
            throw new InvalidOperationException("Chỉ có thể xóa phụ tùng khi Inspection đang Pending hoặc InProgress.");

        // Xác định service inspection hợp lệ
        var serviceInspection = inspection.ServiceInspections.FirstOrDefault(si => si.ServiceId == serviceId);
        if (serviceInspection == null)
            throw new InvalidOperationException("Không tìm thấy dịch vụ này trong Inspection.");

        // Tìm bản ghi PartInspection cần xóa
        var partInspection = inspection.PartInspections.FirstOrDefault(pi => pi.PartInspectionId == partInspectionId);
        if (partInspection == null)
            throw new InvalidOperationException("Không tìm thấy phụ tùng trong Inspection.");

        // Kiểm tra part có thuộc service đó không
        var validPartIds = serviceInspection.Service.ServiceParts.Select(sp => sp.PartId).ToList();
        if (!validPartIds.Contains(partInspection.PartId))
            throw new InvalidOperationException("Phụ tùng không thuộc dịch vụ được chọn.");
        // Xóa partInspection
        _repo.RemovePartInspections(new List<PartInspection> { partInspection });
        await _repo.SaveChangesAsync();

        return true;
    }


    private void AttachSuggestedParts(InspectionTechnicianDto dto, Inspection? entity)
    {
        if (dto == null || entity == null) return;
        if (dto.ServiceInspections == null || dto.ServiceInspections.Count == 0) return;

        // Build a lookup from partInspections by partId
        var partInsByPartId = (entity.PartInspections ?? Enumerable.Empty<PartInspection>())
            .GroupBy(pi => pi.PartId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var sDto in dto.ServiceInspections)
        {
            sDto.SuggestedParts = new List<PartInspectionDto>();

            var sEntity = entity.ServiceInspections?.FirstOrDefault(si => si.ServiceInspectionId == sDto.ServiceInspectionId);
            if (sEntity?.Service == null) continue;

            var allowedPartIds = sEntity.Service.ServiceParts?.Select(sp => sp.PartId).ToHashSet() ?? new HashSet<Guid>();

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

        var servicePartIds = serviceInspection.Service.ServiceParts
            .Select(sp => sp.PartId)
            .ToList();

        var relatedPartInspections = inspection.PartInspections
            .Where(pi => servicePartIds.Contains(pi.PartId))
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
