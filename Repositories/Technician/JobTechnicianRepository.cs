using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Technician
{
    public class JobTechnicianRepository : IJobTechnicianRepository
    {
        private readonly MyAppDbContext _context;

        public JobTechnicianRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Job>> GetJobsByTechnicianAsync(string userId)
        {
            return await _context.JobTechnicians
                .Where(jt => jt.Technician.UserId == userId)
                .Include(jt => jt.Job)
                    .ThenInclude(j => j.Service)
                .Include(jt => jt.Job)
                    .ThenInclude(j => j.RepairOrder)
                .Include(jt => jt.Job)
                    .ThenInclude(j => j.JobParts)
                        .ThenInclude(jp => jp.Part)
                .Include(jt => jt.Job)
                    .ThenInclude(j => j.Repairs)
                .Select(jt => jt.Job)
                .ToListAsync();
        }
    }
}
