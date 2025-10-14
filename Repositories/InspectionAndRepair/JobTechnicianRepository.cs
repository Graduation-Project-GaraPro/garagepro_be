using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.InspectionAndRepair
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
                    .ThenInclude(j => j.Service)  // Để lấy ServiceName
                .Include(jt => jt.Job)
                    .ThenInclude(j => j.RepairOrder)  // Để lấy RepairOrderId
                        .ThenInclude(ro => ro.Vehicle)  // Thêm: Lấy Vehicle từ RepairOrder
                            .ThenInclude(v => v.User)  // Thêm: Lấy Customer (User) từ Vehicle
                .Include(jt => jt.Job)
                    .ThenInclude(j => j.JobParts)
                        .ThenInclude(jp => jp.Part)  // Để lấy Parts
                .Include(jt => jt.Job)
                    .ThenInclude(j => j.Repair)  // Để lấy Repairs (đã có, nhưng đảm bảo load đầy đủ)
                .Select(jt => jt.Job)
                .ToListAsync();
        }
        public async Task UpdateJobAsync(Job job)
        {
            _context.Jobs.Update(job);
            await _context.SaveChangesAsync();
        }

    }
}
