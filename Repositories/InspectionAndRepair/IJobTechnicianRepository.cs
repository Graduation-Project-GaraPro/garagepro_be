using BusinessObject;
using Dtos.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.InspectionAndRepair
{
    public interface IJobTechnicianRepository
    {
        Task<List<Job>> GetJobsByTechnicianAsync(string userId);
        Task UpdateJobAsync(Job job);

    }
}
