using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Roles
{
    public class Permission
    {
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!; // VD: "ViewUsers"
        [Required]
        public string Code { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public bool Deprecated { get; set; } = false;

        public bool IsDefault { get; set; } = false;
        public Guid CategoryId { get; set; }
        public PermissionCategory Category { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
