using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
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

        public async Task<bool> AssignInspectionToTechnicianAsync(Guid inspectionId, Guid technicianId)
        {
            var inspection = await _context.Inspections.FindAsync(inspectionId);
            if (inspection == null) return false;

            inspection.TechnicianId = technicianId;
            inspection.Status = InspectionStatus.InProgress;
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

        public async Task<bool> UpdateInspectionFindingAsync(Guid inspectionId, string finding, string note, IssueRating rating)
        {
            var inspection = await _context.Inspections.FindAsync(inspectionId);
            if (inspection == null) return false;

            inspection.Finding = finding;
            inspection.Note = note;
            inspection.IssueRating = rating;
            inspection.Status = InspectionStatus.Completed;
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

        public async Task<bool> UpdateCustomerConcernAsync(Guid inspectionId, string concern)
        {
            var inspection = await _context.Inspections.FindAsync(inspectionId);
            if (inspection == null) return false;

            inspection.CustomerConcern = concern;
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

        public async Task<bool> UpdateInspectionPriceAsync(Guid inspectionId, decimal price)
        {
            var inspection = await _context.Inspections.FindAsync(inspectionId);
            if (inspection == null) return false;

            inspection.InspectionPrice = price;
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
        
        public async Task<bool> UpdateInspectionTypeAsync(Guid inspectionId, InspectionType type)
        {
            var inspection = await _context.Inspections.FindAsync(inspectionId);
            if (inspection == null) return false;

            inspection.InspectionType = type;
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
    }
}