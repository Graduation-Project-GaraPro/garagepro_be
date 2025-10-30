using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Roles
{
    public class UpdateRoleDto
    {
        [Required(ErrorMessage = "RoleId is required.")]
        public string RoleId { get; set; } = null!;

        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 100 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(500, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "PermissionIds cannot be empty.")]
        public IEnumerable<Guid> PermissionIds { get; set; } = new List<Guid>();

        [Required(ErrorMessage = "GrantedBy is required.")]
        public string GrantedBy { get; set; } = null!;
        public string? GrantedUserId { get; set; }
    }
}
