using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;

namespace Dtos.Branches
{
    public class BranchReadDto
    {
        public Guid BranchId { get; set; }
        public string BranchName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        // Địa chỉ đầy đủ
        public string Street { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string City { get; set; }

        public string Description { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Danh sách service
        public IEnumerable<ServiceDto> Services { get; set; } = new List<ServiceDto>();
        public virtual ICollection<ApplicationUserDto> Staffs { get; set; } = new List<ApplicationUserDto>();
        // Giờ hoạt động
        public IEnumerable<OperatingHourDto> OperatingHours { get; set; } = new List<OperatingHourDto>();
    }

}
