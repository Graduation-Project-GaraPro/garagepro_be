using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Logs
{
    public class LogStatistics
    {
        public int Total { get; set; }
        public int Errors { get; set; }
        public int Warnings { get; set; }
        public int Info { get; set; }
        public int Debug { get; set; }
        public int Critical { get; set; }
        public int Today { get; set; }
        public int ThisWeek { get; set; }
        public int ThisMonth { get; set; }
    }
}
