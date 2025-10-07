using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Enums
{
    public enum InspectionStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        ReviewRequired = 3,//customer review and update 
        Approved = 4
    }
}