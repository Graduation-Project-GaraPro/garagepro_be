using BusinessObject;
using BusinessObject.Enums;
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
        //Task UpdateJobAsync(Job job);
        Task UpdateJobStatusAsync(Guid jobId, JobStatus newStatus, DateTime? endTime = null, TimeSpan? actualTime = null);

    }
}
