using Dtos.Common;
using Dtos.InspectionAndRepair;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.InspectionAndRepair
{
    public interface IJobTechnicianService
    {
        Task<List<JobTechnicianDto>> GetJobsByTechnicianAsync(string userId);
        Task<JobTechnicianDto?> GetJobByIdAsync(string userId, Guid jobId);
        Task<bool> UpdateJobStatusAsync(string userId, JobStatusUpdateDto dto);
        Task<TechnicianDto?> GetTechnicianByUserIdAsync(string userId);

    }
}
