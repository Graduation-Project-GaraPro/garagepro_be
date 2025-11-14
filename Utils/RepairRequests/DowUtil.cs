using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Enums;

namespace Utils.RepairRequests
{
    public static class DowUtil
    {
        // System.DayOfWeek: Sunday=0..Saturday=6
        // DayOfWeekEnum:    Monday=1..Sunday=7
        public static DayOfWeekEnum ToCustomDow(DayOfWeek dotnetDow)
        {
            var i = (int)dotnetDow; // Sunday=0
            return (DayOfWeekEnum)(i == 0 ? 7 : i);
        }
    }
}
