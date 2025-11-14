using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.RepairRequests
{
    public static class SlotWindowUtil
{
    public static (DateTimeOffset openLocal, DateTimeOffset closeLocal) BuildOpenCloseLocal(
        DateOnly date, TimeSpan open, TimeSpan close)
    {
        var openLocal  = VietnamTime.AtVietnamTime(date, open);
        var closeLocal = VietnamTime.AtVietnamTime(date, close);
        if (closeLocal <= openLocal) closeLocal = closeLocal.AddDays(1); // ca đêm
        return (openLocal, closeLocal);
    }

    public static IReadOnlyList<(DateTimeOffset start, DateTimeOffset end)> GenerateWindows(
        DateTimeOffset openLocal, DateTimeOffset closeLocal, int windowMinutes)
    {
        var list = new List<(DateTimeOffset, DateTimeOffset)>();
        for (var t = openLocal; t < closeLocal; t = t.AddMinutes(windowMinutes))
        {
            var end = t.AddMinutes(windowMinutes);
            if (end <= closeLocal) list.Add((t, end));
        }
        return list;
    }

    public static void EnsureInsideOpenHours(
        DateTimeOffset localStart, int windowMinutes,
        DateTimeOffset openLocal, DateTimeOffset closeLocal,
        string errorMessage = "Thời gian chọn nằm ngoài giờ làm việc của chi nhánh.")
    {
        var localEnd = localStart.AddMinutes(windowMinutes);
        if (localStart < openLocal || localEnd > closeLocal)
            throw new Exception(errorMessage);
    }
}
}
