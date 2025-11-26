using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Roles
{
    public class AssignRoleToUsersDto
    {
        public string RoleId { get; set; } = null!;

        
        public List<string> UserIds { get; set; } = new();

        
        public string GrantedBy { get; set; } = null!;
    }
}
