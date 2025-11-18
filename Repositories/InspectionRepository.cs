using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class InspectionRepository : IInspectionRepository
    {
        private readonly MyAppDbContext _context;

        public InspectionRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<Inspection> GetByIdAsync(Guid inspectionId)
        {
            return await _context.Inspections
                .Include(i => i.RepairOrder)
                .Include(i => i.Technician)
                    .ThenInclude(t => t.User)
                .Include(i => i.ServiceInspections)
                    .ThenInclude(si => si.Service)
                        .ThenInclude(s => s.ServiceParts)
                .Include(i => i.PartInspections)
                    .ThenInclude(pi => pi.Part)
                .FirstOrDefaultAsync(i => i.InspectionId == inspectionId);
        }

        public async Task<IEnumerable<Inspection>> GetAllAsync()
        {
            return await _context.Inspections
                .Include(i => i.RepairOrder)
                .Include(i => i.Technician)
                    .ThenInclude(t => t.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inspection>> GetByRepairOrderIdAsync(Guid repairOrderId)
        {
            return await _context.Inspections
                .Include(i => i.RepairOrder)
                .Include(i => i.Technician)
                    .ThenInclude(t => t.User)
                .Include(i => i.ServiceInspections)
                    .ThenInclude(si => si.Service)
                .Include(i => i.PartInspections)
                    .ThenInclude(pi => pi.Part)
                .Where(i => i.RepairOrderId == repairOrderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inspection>> GetByTechnicianIdAsync(Guid technicianId)
        {
            return await _context.Inspections
                .Include(i => i.RepairOrder)
                .Include(i => i.Technician)
                    .ThenInclude(t => t.User)
                .Where(i => i.TechnicianId == technicianId)
                .ToListAsync();
        }

        public async Task<Inspection> CreateAsync(Inspection inspection)
        {
            inspection.CreatedAt = DateTime.UtcNow;
            inspection.UpdatedAt = DateTime.UtcNow;
            _context.Inspections.Add(inspection);
            await _context.SaveChangesAsync();
            return inspection;
        }

        public async Task<Inspection> UpdateAsync(Inspection inspection)
        {
            inspection.UpdatedAt = DateTime.UtcNow;
            _context.Inspections.Update(inspection);
            await _context.SaveChangesAsync();
            return inspection;
        }

        public async Task<bool> DeleteAsync(Guid inspectionId)
        {
            var inspection = await _context.Inspections.FindAsync(inspectionId);
            if (inspection == null) return false;

            _context.Inspections.Remove(inspection);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid inspectionId)
        {
            return await _context.Inspections.AnyAsync(i => i.InspectionId == inspectionId);
        }

        public async Task<IEnumerable<Inspection>> GetPendingInspectionsAsync()
        {
            return await _context.Inspections
                .Include(i => i.RepairOrder)
                .Include(i => i.Technician)
                    .ThenInclude(t => t.User)
                .Where(i => i.Status == InspectionStatus.Pending)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inspection>> GetCompletedInspectionsAsync()
        {
            return await _context.Inspections
                .Include(i => i.RepairOrder)
                .Include(i => i.Technician)
                    .ThenInclude(t => t.User)
                .Where(i => i.Status == InspectionStatus.Completed)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inspection>> GetCompletedInspectionsWithDetailsAsync()
        {
            return await _context.Inspections
                .Include(i => i.RepairOrder)
                .Include(i => i.Technician)
                    .ThenInclude(t => t.User)
                .Include(i => i.ServiceInspections)
                    .ThenInclude(si => si.Service)
                .Include(i => i.PartInspections)
                    .ThenInclude(pi => pi.Part)
                .Where(i => i.Status == InspectionStatus.Completed)
                .ToListAsync();
        }
        public async Task<Inspection?> GetInspectionByIdAsync(Guid inspectionId)
        {
            return await _context.Inspections
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.Brand)
                .Include(i => i.RepairOrder)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.Model)
                .Include(i => i.ServiceInspections)
                    .ThenInclude(si => si.Service)
                .FirstOrDefaultAsync(i => i.InspectionId == inspectionId);
        }
        public async Task<bool> AssignInspectionToTechnicianAsync(Guid inspectionId, Guid technicianId)
        {
            var inspection = await _context.Inspections.FindAsync(inspectionId);
            if (inspection == null) return false;

            inspection.TechnicianId = technicianId;
            // Change status from pending to new when assigning technician
            if (inspection.Status == InspectionStatus.Pending)
            {
                inspection.Status = InspectionStatus.New;
            }
            inspection.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Technician?> GetTechnicianByIdAsync(Guid technicianId)
        {
            return await _context.Technicians
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TechnicianId == technicianId);
        }

    }
}