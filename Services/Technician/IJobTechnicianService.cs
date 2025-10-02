using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Technician
{
    public interface IJobTechnicianService
    {
        Task<List<Job>> GetJobsByTechnicianAsync(string userId);
    }
}
