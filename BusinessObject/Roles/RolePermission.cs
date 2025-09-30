using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;
using Microsoft.AspNetCore.Identity;

namespace BusinessObject.Roles
{
    public class RolePermission
    {
        // Khóa chính gồm RoleId + PermissionId (composite key)
        public string RoleId { get; set; } = null!;
        public Guid PermissionId { get; set; }

        // Quan hệ tới Role
        public ApplicationRole Role { get; set; } = null!;

        // Quan hệ tới Permission
        public Permission Permission { get; set; } = null!;

        
        public string GrantedBy { get; set; } = null!;

        [ForeignKey(nameof(GrantedBy))]
        public ApplicationUser User { get; set; } = null!;

        // Ngày giờ gán quyền
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;


    }
}
