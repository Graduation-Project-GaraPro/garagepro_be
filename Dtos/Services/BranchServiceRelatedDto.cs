using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Services
{
    public class BranchServiceRelatedDto
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
    }
}
