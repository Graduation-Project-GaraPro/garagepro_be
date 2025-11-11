using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Customers
{
    public record SlotAvailabilityDto
    {
        public DateTimeOffset WindowStart { get; init; }   // mốc đầu khung giờ
        public DateTimeOffset WindowEnd { get; init; }   // mốc cuối khung giờ
        public int Capacity { get; init; }   // MaxBookingsPerWindow
        public int ApprovedCount { get; init; }   // số đã duyệt (Accept)
        public int Remaining { get; init; }   // còn trống = Capacity - ApprovedCount
        public bool IsFull { get; init; }   // Remaining == 0
    }
}
