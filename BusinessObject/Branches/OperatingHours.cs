using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Branches
{
    [Owned]
    public class DaySchedule
    {
        public bool IsOpen { get; set; } = false;
        public string OpenTime { get; set; } = "08:00";
        public string CloseTime { get; set; } = "17:00";
    }

    [Owned]
    public class OperatingHours
    {
        public DaySchedule Monday { get; set; } = new DaySchedule();
        public DaySchedule Tuesday { get; set; } = new DaySchedule();
        public DaySchedule Wednesday { get; set; } = new DaySchedule();
        public DaySchedule Thursday { get; set; } = new DaySchedule();
        public DaySchedule Friday { get; set; } = new DaySchedule();
        public DaySchedule Saturday { get; set; } = new DaySchedule();
        public DaySchedule Sunday { get; set; } = new DaySchedule();
    }
}
