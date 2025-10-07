using BusinessObject.Enums;
using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Dtos.Technician;

namespace Repositories.Technician
{
    public class InspectionTechnicianRepository : IInspectionTechnicianRepository
    {
        private readonly MyAppDbContext _context;

        public InspectionTechnicianRepository(MyAppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách Inspection của Technician
        /// Chỉ lấy các inspection có trạng thái: New, Pending, InProgress, Completed
        /// </summary>
        public async Task<List<Inspection>> GetInspectionsByTechnicianAsync(string userId)
        {
            // Lấy technicianId của user hiện tại
            var technicianId = await _context.Technicians
                .Where(t => t.UserId == userId)
                .Select(t => t.TechnicianId)
                .FirstOrDefaultAsync();

            if (technicianId == Guid.Empty)
            {
                return new List<Inspection>();
            }

            // Lấy tất cả inspection của technician này
            return await _context.Inspections
                .Where(i => i.TechnicianId == technicianId) // Chỉ lấy inspection được giao cho technician này
                .Include(i => i.ServiceInspections)
                    .ThenInclude(si => si.Service)
                .Include(i => i.PartInspections)
                    .ThenInclude(pi => pi.Part)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.User)
                 .Include(i => i.RepairOrder)
            .ThenInclude(ro => ro.RepairOrderServices)
                .ThenInclude(ros => ros.Service)
        .OrderByDescending(i => i.CreatedAt)
        .ToListAsync();
        }


        /// <summary>
        /// Lấy chi tiết một Inspection
        /// Chỉ cho phép Technician được giao xem
        /// </summary>
        public async Task<Inspection> GetInspectionByIdAsync(Guid id, string userId)
        {
            var technicianId = await _context.Technicians
                .Where(t => t.UserId == userId)
                .Select(t => t.TechnicianId)
                .FirstOrDefaultAsync();

            if (technicianId == Guid.Empty)
            {
                return null;
            }

            return await _context.Inspections
                .Where(i => i.InspectionId == id && i.TechnicianId == technicianId)
                .Include(i => i.ServiceInspections)
                    .ThenInclude(si => si.Service)
                .Include(i => i.PartInspections)
                    .ThenInclude(pi => pi.Part)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.User)
                 .Include(i => i.RepairOrder)
            .ThenInclude(ro => ro.RepairOrderServices)
                .ThenInclude(ros => ros.Service)
        .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Cập nhật kết quả kiểm tra
        /// - Chỉ cho phép cập nhật khi trạng thái là New, Pending, InProgress
        /// - Technician viết Finding và gợi ý Parts
        /// - Nếu IsCompleted = true, chuyển từ New sang Pending để Manager review
        /// </summary>
        public async Task<Inspection> UpdateInspectionAsync(Guid id, UpdateInspectionRequest request, string userId)
        {
            var inspection = await GetInspectionByIdAsync(id, userId);
            if (inspection == null)
            {
                throw new InvalidOperationException("Không tìm thấy Inspection hoặc bạn không có quyền cập nhật.");
            }

            // Chỉ cho phép cập nhật khi còn trong 3 trạng thái đầu
            if (inspection.Status != InspectionStatus.New &&
                inspection.Status != InspectionStatus.Pending &&
                inspection.Status != InspectionStatus.InProgress)
            {
                throw new InvalidOperationException("Không thể cập nhật khi Inspection đã hoàn thành hoặc bị khóa.");
            }

            // Cập nhật mô tả kết quả kiểm tra
            inspection.Finding = request.Finding;
            inspection.UpdatedAt = DateTime.UtcNow;

            // === FIX: Lấy danh sách Service từ RepairOrder thay vì ServiceInspections ===
            var repairOrderServices = await _context.RepairOrderServices
                .Include(ros => ros.Service)
                    .ThenInclude(s => s.ServiceParts)
                        .ThenInclude(sp => sp.Part)
                .Where(ros => ros.RepairOrderId == inspection.RepairOrderId)
                .ToListAsync();

            if (!repairOrderServices.Any())
            {
                throw new InvalidOperationException("RepairOrder này không có dịch vụ nào để kiểm tra.");
            }

            // Lấy tất cả ServiceInspections hiện có của Inspection này
            var existingServiceInspections = await _context.ServiceInspections
                .Where(si => si.InspectionId == id)
                .ToDictionaryAsync(si => si.ServiceId, si => si);

            // Xử lý từng Service trong request
            foreach (var serviceUpdate in request.ServiceUpdates)
            {
                // Kiểm tra ServiceId có tồn tại trong RepairOrderServices không
                var repairOrderService = repairOrderServices
                    .FirstOrDefault(ros => ros.ServiceId == serviceUpdate.ServiceId);

                if (repairOrderService == null)
                {
                    // Lấy tên service để hiển thị lỗi rõ ràng
                    var requestedService = await _context.Services
                        .Where(s => s.ServiceId == serviceUpdate.ServiceId)
                        .Select(s => s.ServiceName)
                        .FirstOrDefaultAsync();

                    var availableServices = repairOrderServices
                        .Select(ros => $"{ros.Service?.ServiceName} (ID: {ros.ServiceId})")
                        .ToList();

                    throw new InvalidOperationException(
                        $"ServiceId {serviceUpdate.ServiceId} " +
                        $"('{requestedService ?? "Unknown"}') không có trong RepairOrder này.\n" +
                        $"Các Service có sẵn: {string.Join(", ", availableServices)}");
                }

                // Tìm hoặc tạo mới ServiceInspection
                ServiceInspection serviceInspection;

                if (existingServiceInspections.ContainsKey(serviceUpdate.ServiceId))
                {
                    // Đã có ServiceInspection -> cập nhật
                    serviceInspection = existingServiceInspections[serviceUpdate.ServiceId];
                    serviceInspection.ConditionStatus = serviceUpdate.ConditionStatus;
                }
                else
                {
                    // Chưa có ServiceInspection -> tạo mới
                    serviceInspection = new ServiceInspection
                    {
                        ServiceInspectionId = Guid.NewGuid(),
                        InspectionId = id,
                        ServiceId = serviceUpdate.ServiceId,
                        ConditionStatus = serviceUpdate.ConditionStatus,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ServiceInspections.Add(serviceInspection);
                }

                // === Xử lý gợi ý phụ tùng ===
                if (serviceUpdate.SuggestedPartIds != null && serviceUpdate.SuggestedPartIds.Any())
                {
                    // Kiểm tra trạng thái phù hợp để gợi ý phụ tùng
                    if (serviceUpdate.ConditionStatus != ConditionStatus.Replace &&
                        serviceUpdate.ConditionStatus != ConditionStatus.Needs_Attention)
                    {
                        throw new InvalidOperationException(
                            $"Không thể gợi ý phụ tùng cho dịch vụ '{repairOrderService.Service?.ServiceName}' " +
                            $"khi tình trạng là '{serviceUpdate.ConditionStatus}'. " +
                            $"Chỉ cho phép khi là 'Replace' hoặc 'Needs_Attention'.");
                    }

                    // Xóa các PartInspection cũ của service này
                    // Bước 1: Lấy danh sách PartId thuộc service
                    var servicePartIds = repairOrderService.Service.ServiceParts
                        .Select(sp => sp.PartId)
                        .ToList();

                    // Bước 2: Query PartInspections với danh sách PartId đã lấy
                    var oldPartInspections = await _context.PartInspections
                        .Where(pi => pi.InspectionId == id && servicePartIds.Contains(pi.PartId))
                        .ToListAsync();

                    if (oldPartInspections.Any())
                    {
                        _context.PartInspections.RemoveRange(oldPartInspections);
                    }

                    // Validate và thêm phụ tùng mới
                    foreach (var partId in serviceUpdate.SuggestedPartIds)
                    {
                        // Kiểm tra Part có thuộc Service không
                        bool isValidPart = repairOrderService.Service.ServiceParts
                            .Any(sp => sp.PartId == partId);

                        if (!isValidPart)
                        {
                            var partInfo = await _context.Parts
                                .Where(p => p.PartId == partId)
                                .Select(p => p.Name)
                                .FirstOrDefaultAsync();

                            var validParts = repairOrderService.Service.ServiceParts
                                .Select(sp => $"{sp.Part?.Name ?? "Unknown"} (ID: {sp.PartId})")
                                .ToList();

                            throw new InvalidOperationException(
                                $"Phụ tùng '{partInfo ?? partId.ToString()}' " +
                                $"không thuộc dịch vụ '{repairOrderService.Service?.ServiceName}'.\n" +
                                $"Các phụ tùng hợp lệ cho dịch vụ này: " +
                                $"{(validParts.Any() ? string.Join(", ", validParts) : "Không có")}");
                        }

                        // Thêm PartInspection mới
                        _context.PartInspections.Add(new PartInspection
                        {
                            PartInspectionId = Guid.NewGuid(),
                            InspectionId = id,
                            PartId = partId,
                            Status = "Suggested",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            // Cập nhật trạng thái Inspection
            if (request.IsCompleted)
            {
                if (inspection.Status == InspectionStatus.New)
                    inspection.Status = InspectionStatus.Pending;
                else if (inspection.Status == InspectionStatus.InProgress)
                    inspection.Status = InspectionStatus.Pending;
            }
            else
            {
                if (inspection.Status == InspectionStatus.New)
                    inspection.Status = InspectionStatus.InProgress;
            }

            await _context.SaveChangesAsync();

            // Trả về inspection đã cập nhật đầy đủ
            return await GetInspectionByIdAsync(id, userId);
        }

        /// <summary>
        /// Bắt đầu kiểm tra (chuyển từ New sang InProgress)
        /// </summary>
        public async Task<Inspection> StartInspectionAsync(Guid id, string userId)
        {
            var inspection = await GetInspectionByIdAsync(id, userId);
            if (inspection == null)
            {
                return null;
            }

            // Chỉ cho phép start khi trạng thái là New
            if (inspection.Status != InspectionStatus.New)
            {
                return null;
            }

            inspection.Status = InspectionStatus.InProgress;
            inspection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return inspection;
        }
    }
}