using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Enums
{
    public enum JobStatus
    {
        Pending = 0,           // Initial status when job is created
        AssignedToTechnician = 1,  // Manager assigned job to technician
        InProgress = 2,        // Technician started working on job
        Completed = 3          // Job completed
    }
}