using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Roles
{
    public class PermissionCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!; // VD: "User Management"
        public string? Description { get; set; }

        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
