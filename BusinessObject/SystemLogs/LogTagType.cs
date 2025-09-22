using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.SystemLogs
{
    public enum LogTagType
    {
        Unknown = 0,
        Performance = 1,
        Security = 2,
        Database = 3,
        Network = 4,
        UI = 5,
        BusinessLogic = 6
    }
}
