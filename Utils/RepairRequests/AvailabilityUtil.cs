using Dtos.Customers;

namespace Utils.RepairRequests
{
    public static class AvailabilityUtil
    {
        /// <summary>Group các ArrivalWindowStart (UTC hoặc offset bất kỳ) theo slot LOCAL (VN) và đếm.</summary>
        public static Dictionary<DateTimeOffset, int> GroupAcceptsBySlot(
        IEnumerable<DateTimeOffset> arrivalList, int windowMinutes)
        {
            return arrivalList
                .GroupBy(ts => VietnamTime.NormalizeWindow(ts, windowMinutes)) // trực tiếp vì đều +07:00
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public static IReadOnlyList<SlotAvailabilityDto> Build(
            IEnumerable<(DateTimeOffset start, DateTimeOffset end)> windows,
            Dictionary<DateTimeOffset, int> usedMap,
            int capacityPerWindow)
        {
            var result = new List<SlotAvailabilityDto>();
            foreach (var (start, end) in windows)
            {
                var key = VietnamTime.NormalizeWindow(start, (int)(end - start).TotalMinutes);
                var used = usedMap.TryGetValue(key, out var c) ? c : 0;
                var remaining = Math.Max(0, capacityPerWindow - used);

                result.Add(new SlotAvailabilityDto
                {
                    WindowStart = start,
                    WindowEnd = end,
                    Capacity = capacityPerWindow,
                    ApprovedCount = used,
                    Remaining = remaining,
                    IsFull = remaining == 0
                });
            }
            return result;
        }
    }

}
