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
        WaitingCustomerApproval = 1,  // Job sent to customer for approval
        CustomerApproved = 2,  // Customer approved the job
        CustomerRejected = 3,  // Customer rejected the job
        AssignedToTechnician = 4,  // Manager assigned job to technician
        InProgress = 5,        // Technician started working on job
        Completed = 6          // Job completed
    }
}