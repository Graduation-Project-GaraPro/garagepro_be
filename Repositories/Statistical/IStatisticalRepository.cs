using BusinessObject.InspectionAndRepair;
using System;
using System.Threading.Tasks;

namespace Repositories.Statistical
{
    public interface IStatisticalRepository
    {
        Task<Technician> GetTechnicianWithJobsAsync(Guid technicianId);
    }
}
