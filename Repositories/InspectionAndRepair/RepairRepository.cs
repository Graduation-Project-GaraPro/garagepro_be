using BusinessObject.InspectionAndRepair;
using BusinessObject;
using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Repositories.InspectionAndRepair
{
    public class RepairRepository : IRepairRepository
    {
        private readonly MyAppDbContext _context;

        public RepairRepository(MyAppDbContext context)
        {
            _context = context;
        }
        public async Task<RepairOrder> GetRepairOrderWithJobsAsync(Guid repairOrderId)
        {
            return await _context.RepairOrders
                .Include(r => r.Vehicle)
                .Include(r => r.User)
                .Include(r => r.Jobs)
                    .ThenInclude(j => j.Service)
                .Include(r => r.Jobs)
                    .ThenInclude(j => j.JobTechnicians)
                .Include(r => r.Jobs)
                    .ThenInclude(j => j.JobParts)
                        .ThenInclude(jp => jp.Part)
                .Include(r => r.Jobs)
                    .ThenInclude(j => j.Repair) 
                .FirstOrDefaultAsync(r => r.RepairOrderId == repairOrderId);
        }

        public async Task<Repair> GetRepairByIdAsync(Guid repairId)
        {
            return await _context.Repairs
                .Include(r => r.Job)
                    .ThenInclude(j => j.Service)
                .FirstOrDefaultAsync(r => r.RepairId == repairId);
        }

        public async Task<Job> GetJobByIdAsync(Guid jobId)
        {
            return await _context.Jobs
                .Include(j => j.JobTechnicians)
                .Include(j => j.Service)
                .FirstOrDefaultAsync(j => j.JobId == jobId);
        }

        public async Task<bool> TechnicianHasJobAsync(Guid technicianId, Guid jobId)
        {
            return await _context.JobTechnicians
                .AnyAsync(jt => jt.TechnicianId == technicianId && jt.JobId == jobId);
        }

        public async Task AddRepairAsync(Repair repair)
        {
            await _context.Repairs.AddAsync(repair);
        }

        public async Task UpdateRepairAsync(Repair repair)
        {
            _context.Repairs.Update(repair);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
