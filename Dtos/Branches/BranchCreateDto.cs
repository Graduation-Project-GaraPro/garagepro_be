using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Branches
{
    public class BranchCreateDto
    {
        public string BranchName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Street { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string Description { get; set; }

        public List<Guid> ServiceIds { get; set; } = new();
        public List<string> StaffIds { get; set; } = new();
        public List<OperatingHourDto> OperatingHours { get; set; } = new();
    }
}
