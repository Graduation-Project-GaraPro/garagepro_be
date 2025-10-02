using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Roles
{
    public class UpdateRoleDto
    {
        public string RoleId { get; set; } = null!;
        public string? NewName { get; set; }
        public string? Description { get; set; }
        public IEnumerable<Guid> PermissionIds { get; set; } = new List<Guid>();
        public string GrantedBy { get; set; } = null!;
        public string? GrantedUserId { get; set; }
    }
}
