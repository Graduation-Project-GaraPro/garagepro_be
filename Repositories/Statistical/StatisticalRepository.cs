using BusinessObject.InspectionAndRepair;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Repositories.Statistical
{
    public class StatisticalRepository : IStatisticalRepository
    {
        private readonly MyAppDbContext _context;

        public StatisticalRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<Technician> GetTechnicianByUserIdAsync(string userId)
        {
            return await _context.Technicians
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }
        public async Task<Technician> GetTechnicianWithJobsAsync(Guid technicianId)
        {
            return await _context.Technicians
                .Include(t => t.JobTechnicians)
                    .ThenInclude(jt => jt.Job)
                        .ThenInclude(j => j.RepairOrder)
                            .ThenInclude(ro => ro.Vehicle)
                .FirstOrDefaultAsync(t => t.TechnicianId == technicianId);
        }
    }
}
