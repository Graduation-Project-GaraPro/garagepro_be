using BusinessObject.InspectionAndRepair;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.InspectionAndRepair
{
    public class TechnicianRepository : ITechnicianRepository
    {
        private readonly MyAppDbContext _context;

        public TechnicianRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<Technician?> GetByIdAsync(Guid technicianId)
        {
            return await _context.Technicians
                .Include(t => t.User)
                .Include(t => t.JobTechnicians)
                .FirstOrDefaultAsync(t => t.TechnicianId == technicianId);
        }

        public async Task<List<Technician>> GetAllAsync()
        {
            return await _context.Technicians
                .Include(t => t.User)
                .Include(t => t.JobTechnicians)
                .ToListAsync();
        }

        public async Task<Technician?> GetByUserIdAsync(string userId)
        {
            return await _context.Technicians
                .Include(t => t.User)
                .Include(t => t.JobTechnicians)
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task<Technician> CreateAsync(Technician technician)
        {
            _context.Technicians.Add(technician);
            await _context.SaveChangesAsync();
            return technician;
        }

        public async Task<Technician> UpdateAsync(Technician technician)
        {
            _context.Technicians.Update(technician);
            await _context.SaveChangesAsync();
            return technician;
        }

        public async Task<bool> DeleteAsync(Guid technicianId)
        {
            var technician = await _context.Technicians.FindAsync(technicianId);
            if (technician == null)
                return false;

            _context.Technicians.Remove(technician);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
