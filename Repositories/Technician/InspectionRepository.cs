using BusinessObject.Enums;
using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Dtos.Technician;

namespace Repositories.Technician
{
    public class InspectionRepository : IInspectionRepository
    {
        private readonly MyAppDbContext _context;

        public InspectionRepository(MyAppDbContext context)
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
                .Where(i => i.TechnicianId == technicianId) // 👈 Chỉ lấy inspection được giao cho technician này
                .Include(i => i.ServiceInspections)
                    .ThenInclude(si => si.Service)
                .Include(i => i.PartInspections)
                    .ThenInclude(pi => pi.Part)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.User)
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
                return null;
            }

            // Kiểm tra trạng thái: chỉ cho phép cập nhật khi New, Pending, InProgress
            if (inspection.Status != InspectionStatus.New &&
                inspection.Status != InspectionStatus.Pending &&
                inspection.Status != InspectionStatus.InProgress)
            {
                return null; // Không cho phép cập nhật các trạng thái khác
            }

            // Cập nhật Finding (kết quả kiểm tra từ Technician)
            inspection.Finding = request.Finding;

            // Xóa các PartInspection cũ và thêm mới (Parts được Technician gợi ý)
            var existingPartInspections = _context.PartInspections
                .Where(pi => pi.InspectionId == id);
            _context.PartInspections.RemoveRange(existingPartInspections);

            if (request.SuggestedPartIds != null && request.SuggestedPartIds.Any())
            {
                foreach (var partId in request.SuggestedPartIds)
                {
                    _context.PartInspections.Add(new PartInspection
                    {
                        PartInspectionId = Guid.NewGuid(),
                        InspectionId = id,
                        PartId = partId,
                        Status = "Suggested", // Trạng thái đề xuất
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // Nếu hoàn thành kiểm tra, chuyển trạng thái
            if (request.IsCompleted)
            {
                // Từ New -> Pending (gửi cho Manager xem xét)
                if (inspection.Status == InspectionStatus.New)
                {
                    inspection.Status = InspectionStatus.Pending;
                }
            }
            else
            {
                // Nếu đang kiểm tra nhưng chưa hoàn thành, chuyển sang InProgress
                if (inspection.Status == InspectionStatus.New)
                {
                    inspection.Status = InspectionStatus.InProgress;
                }
            }

            inspection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Reload để lấy thông tin đầy đủ
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