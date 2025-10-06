using BusinessObject;
using Repositories.Technician;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Technician
{
    public class JobTechnicianService : IJobTechnicianService
    {
        private readonly IJobTechnicianRepository _jobTechnicianRepository;

        public JobTechnicianService(IJobTechnicianRepository jobTechnicianRepository)
        {
            _jobTechnicianRepository = jobTechnicianRepository;
        }

        public async Task<List<Job>> GetJobsByTechnicianAsync(string userId)
        {
            return await _jobTechnicianRepository.GetJobsByTechnicianAsync(userId);
        }
    }
}
