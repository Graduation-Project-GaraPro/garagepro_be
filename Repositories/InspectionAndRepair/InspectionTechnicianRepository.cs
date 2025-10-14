using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories.InspectionAndRepair
{
    public class InspectionTechnicianRepository : IInspectionTechnicianRepository
    {
        private readonly MyAppDbContext _context;

        public InspectionTechnicianRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<BusinessObject.InspectionAndRepair.Technician?> GetTechnicianByUserIdAsync(string userId)
        {
            return await _context.Technicians
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task<List<Inspection>> GetInspectionsByTechnicianIdAsync(Guid technicianId)
        {
           return await _context.Inspections
                .Where(i => i.TechnicianId == technicianId)
                .Include(i => i.ServiceInspections)
                    .ThenInclude(si => si.Service)
                        .ThenInclude(s => s.ServiceParts)
                            .ThenInclude(sp => sp.Part)
                .Include(i => i.PartInspections).ThenInclude(pi => pi.Part)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.RepairOrderServices)
                        .ThenInclude(ros => ros.Service)
                .Include(i => i.RepairOrder.Vehicle).ThenInclude(v => v.User)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<Inspection?> GetInspectionByIdAndTechnicianIdAsync(Guid id, Guid technicianId)
        {
            return await _context.Inspections
                .Where(i => i.InspectionId == id && i.TechnicianId == technicianId)
                .Include(i => i.ServiceInspections)
                .ThenInclude(si => si.Service)
                .ThenInclude(s => s.ServiceParts)
                    .ThenInclude(sp => sp.Part)
                .Include(i => i.PartInspections).ThenInclude(pi => pi.Part)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.RepairOrderServices)
                        .ThenInclude(ros => ros.Service)
                .Include(i => i.RepairOrder.Vehicle).ThenInclude(v => v.User)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<RepairOrderService>> GetRepairOrderServicesAsync(Guid repairOrderId)
        {
            return await _context.RepairOrderServices
                .Include(ros => ros.Service)
                    .ThenInclude(s => s.ServiceParts)
                        .ThenInclude(sp => sp.Part)
                .Where(ros => ros.RepairOrderId == repairOrderId)
                .ToListAsync();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public void AddServiceInspection(ServiceInspection serviceInspection)
            => _context.ServiceInspections.Add(serviceInspection);

        public void AddPartInspection(PartInspection partInspection)
            => _context.PartInspections.Add(partInspection);

        public void RemovePartInspections(IEnumerable<PartInspection> inspections)
            => _context.PartInspections.RemoveRange(inspections);
    }
}
