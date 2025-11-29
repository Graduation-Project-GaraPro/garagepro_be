using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
       
        public string Commune { get; set; } = string.Empty;

       
        public string Province { get; set; } = string.Empty;
        
        public int ArrivalWindowMinutes { get; set; } = 30;

        public int MaxBookingsPerWindow { get; set; } = 6;

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
