using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Enums;

namespace Dtos.RepairProgressDto
{
    public class RepairOrderFilterDto
    {
        public int? StatusId { get; set; } = null;
        public RoType? RoType { get; set; } = null;
        public string? PaidStatus { get; set; } = null;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
