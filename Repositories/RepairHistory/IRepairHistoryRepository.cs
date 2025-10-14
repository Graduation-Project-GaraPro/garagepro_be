using BusinessObject;
using BusinessObject.InspectionAndRepair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.RepairHistory
{
    public interface IRepairHistoryRepository
    {
        Task<Technician> GetTechnicianWithCompletedJobsAsync(Guid technicianId);
    }
}
