﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.SystemLogs;
using Microsoft.AspNetCore.Identity;
using BusinessObject.Notifications;
namespace BusinessObject.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public Guid RoleId { get; set; }

        [Required]
        public Guid BranchId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public override string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public override string? PhoneNumber { get; set; }

        [MaxLength(255)]
        public override string? PasswordHash { get; set; }

        [MaxLength(200)]
        public string? AvatarUrl { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool Gender { get; set; } // true = Male, false = Female

        public DateTime? Birthday { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        // Legacy properties for compatibility
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public string? Avatar { get; set; }
        public DateTime? dateOfBirth { get; set; }

        public IEnumerable<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();
        public virtual IEnumerable<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual Technician.Technician Technician { get; set; } // Thêm quan hệ với Technician
        // Custom claims
        //public virtual ICollection<ApplicationUserClaim> Claims { get; set; }
    }
}
