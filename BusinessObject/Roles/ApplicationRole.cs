using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BusinessObject.Roles
{
    public class ApplicationRole : IdentityRole // bạn có thể để string hoặc Guid
    {
        public string Description { get; set; }
        public bool IsDefault { get; set; }

        // Các property bổ sung
        public int Users { get; set; } // lưu số lượng user (có thể tính hoặc mapping)
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
