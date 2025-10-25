using BusinessObject.InspectionAndRepair;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.RepairHistory
{
    public class RepairHistoryRepository : IRepairHistoryRepository
    {
        private readonly MyAppDbContext _context;

        public RepairHistoryRepository(MyAppDbContext context)
        {
            _context = context;
        }
        public async Task<Technician> GetTechnicianByUserIdAsync(string userId)
        {
            return await _context.Technicians
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task<Technician> GetTechnicianWithCompletedJobsAsync(Guid technicianId)
        {
            return await _context.Technicians
                .Include(t => t.JobTechnicians)
                    .ThenInclude(jt => jt.Job)
                        .ThenInclude(j => j.JobParts)
                            .ThenInclude(jp => jp.Part)
                .Include(t => t.JobTechnicians)
                    .ThenInclude(jt => jt.Job)
                        .ThenInclude(j => j.RepairOrder)
                            .ThenInclude(ro => ro.Vehicle)
                                .ThenInclude(v => v.User)
                .Include(t => t.JobTechnicians)
                    .ThenInclude(jt => jt.Job)
                        .ThenInclude(j => j.RepairOrder)
                            .ThenInclude(ro => ro.Vehicle)
                                .ThenInclude(v => v.Brand) 
                .Include(t => t.JobTechnicians)
                    .ThenInclude(jt => jt.Job)
                        .ThenInclude(j => j.RepairOrder)
                            .ThenInclude(ro => ro.Vehicle)
                                .ThenInclude(v => v.Model) 
                .Include(t => t.JobTechnicians)
                    .ThenInclude(jt => jt.Job)
                        .ThenInclude(j => j.RepairOrder)
                            .ThenInclude(ro => ro.Vehicle)
                                .ThenInclude(v => v.Color) 
                .Include(t => t.JobTechnicians)
                    .ThenInclude(jt => jt.Job)
                        .ThenInclude(j => j.RepairOrder)
                            .ThenInclude(ro => ro.RepairOrderServices)
                                .ThenInclude(rs => rs.Service)
                .FirstOrDefaultAsync(t => t.TechnicianId == technicianId);
        }
    }
}
