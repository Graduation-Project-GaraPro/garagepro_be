using BusinessObject.InspectionAndRepair;
using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.InspectionAndRepair
{
    public interface IRepairRepository
    {
        Task<RepairOrder> GetRepairOrderWithJobsAsync(Guid repairOrderId);
        Task<Job> GetJobByIdAsync(Guid jobId);
        Task<Repair> GetRepairByIdAsync(Guid repairId);

        Task<bool> TechnicianHasJobAsync(Guid technicianId, Guid jobId);
        Task AddRepairAsync(Repair repair);
        Task UpdateRepairAsync(Repair repair);
        Task SaveChangesAsync();
    }
}
