using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Enums;
namespace Dtos.Branches
{
    public class OperatingHourDto
    {
        public DayOfWeekEnum DayOfWeek { get; set; }
        public bool IsOpen { get; set; } = false;
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
    }
}
