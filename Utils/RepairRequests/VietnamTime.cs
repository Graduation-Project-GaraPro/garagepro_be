using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.RepairRequests
{
    public static class VietnamTime
    {
        public static readonly TimeSpan VN_OFFSET = TimeSpan.FromHours(7);

        public static DateTimeOffset AtVN(DateTimeOffset any)
        => new DateTimeOffset(any.UtcDateTime, VN_OFFSET);

        public static DateTimeOffset AtVietnamTime(DateOnly date, TimeSpan time)
            => new DateTimeOffset(date.Year, date.Month, date.Day, time.Hours, time.Minutes, 0, VN_OFFSET);

        public static DateTimeOffset NormalizeWindow(DateTimeOffset t, int windowMinutes)
        {
            if (windowMinutes <= 0) windowMinutes = 30;
            var epoch = new DateTimeOffset(2000, 1, 1, 0, 0, 0, VN_OFFSET);
            var mins = (long)(t - epoch).TotalMinutes;
            var baseM = mins / windowMinutes * windowMinutes;
            return epoch.AddMinutes(baseM); // luôn giữ offset +07:00
        }
    }
}
