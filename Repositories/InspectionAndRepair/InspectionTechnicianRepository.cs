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
                        .ThenInclude(s => s.ServiceCategory)
                .Include(i => i.ServiceInspections)
                    .ThenInclude(si => si.Service)
                        .ThenInclude(s => s.ServicePartCategories)
                            .ThenInclude(spc => spc.PartCategory)
                                .ThenInclude(pc => pc.Parts)
                .Include(i => i.PartInspections)
                    .ThenInclude(pi => pi.Part)
                .Include(i => i.PartInspections)
                    .ThenInclude(pi => pi.PartCategory)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.RepairOrderServices)
                        .ThenInclude(ros => ros.Service)
                            .ThenInclude(s => s.ServiceCategory)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.RepairOrderServices)
                        .ThenInclude(ros => ros.Service)
                            .ThenInclude(s => s.ServicePartCategories)
                                .ThenInclude(spc => spc.PartCategory)
                                    .ThenInclude(pc => pc.Parts)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.RepairOrderServices)
                        .ThenInclude(ros => ros.RepairOrderServiceParts)
                            .ThenInclude(rosp => rosp.Part)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.Brand)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.Model)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.User)
                .OrderByDescending(i => i.CreatedAt)
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<Inspection?> GetInspectionByIdAndTechnicianIdAsync(Guid id, Guid technicianId)
        {
            return await _context.Inspections
                .Where(i => i.InspectionId == id && i.TechnicianId == technicianId)
                .Include(i => i.ServiceInspections)
                    .ThenInclude(si => si.Service)
                        .ThenInclude(s => s.ServiceCategory)
                .Include(i => i.ServiceInspections)
                    .ThenInclude(si => si.Service)
                        .ThenInclude(s => s.ServicePartCategories)
                            .ThenInclude(spc => spc.PartCategory)
                                .ThenInclude(pc => pc.Parts)
                .Include(i => i.PartInspections)
                    .ThenInclude(pi => pi.Part)
                .Include(i => i.PartInspections)
                    .ThenInclude(pi => pi.PartCategory)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.RepairOrderServices)
                        .ThenInclude(ros => ros.Service)
                            .ThenInclude(s => s.ServiceCategory)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.RepairOrderServices)
                        .ThenInclude(ros => ros.Service)
                            .ThenInclude(s => s.ServicePartCategories)
                                .ThenInclude(spc => spc.PartCategory)
                                    .ThenInclude(pc => pc.Parts)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.RepairOrderServices)
                        .ThenInclude(ros => ros.RepairOrderServiceParts)
                            .ThenInclude(rosp => rosp.Part)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.Brand)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.Model)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.User)
                .AsSplitQuery()
                .FirstOrDefaultAsync();
        }

        public async Task<List<RepairOrderService>> GetRepairOrderServicesAsync(Guid repairOrderId)
        {
            return await _context.RepairOrderServices
                .Include(ros => ros.Service)
                    .ThenInclude(s => s.ServiceCategory)
                .Include(ros => ros.Service)
                    .ThenInclude(s => s.ServicePartCategories)
                        .ThenInclude(spc => spc.PartCategory)
                            .ThenInclude(pc => pc.Parts)
                .Where(ros => ros.RepairOrderId == repairOrderId)
                .ToListAsync();
        }

        public async Task<Service?> GetServiceByIdAsync(Guid serviceId)
        {
            return await _context.Services
                .Include(s => s.ServiceCategory)
                .Include(s => s.ServicePartCategories)
                    .ThenInclude(spc => spc.PartCategory)
                        .ThenInclude(pc => pc.Parts)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId);
        }


        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public void AddServiceInspection(ServiceInspection serviceInspection)
            => _context.ServiceInspections.Add(serviceInspection);

        public void AddPartInspection(PartInspection partInspection)
            => _context.PartInspections.Add(partInspection);

        public void RemovePartInspections(IEnumerable<PartInspection> inspections)
            => _context.PartInspections.RemoveRange(inspections);

        public async Task<bool> HasRepairOrderServicesAsync(Guid repairOrderId)
        {
            return await _context.RepairOrderServices
                .AnyAsync(ros => ros.RepairOrderId == repairOrderId);
        }
        
        public async Task<List<Service>> GetAllServicesAsync()
        {
            return await _context.Services
                .Include(s => s.ServiceCategory)
                .OrderBy(s => s.ServiceName)
                .ToListAsync();
        }
        public void RemoveServiceInspection(ServiceInspection serviceInspection)
            => _context.ServiceInspections.Remove(serviceInspection);
    }
}