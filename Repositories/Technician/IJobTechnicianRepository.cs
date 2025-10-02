using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Technician
{
    public interface IJobTechnicianRepository
    {
        Task<List<Job>> GetJobsByTechnicianAsync(string userId);
    }
}
