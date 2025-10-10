﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.SystemLogs;
using Microsoft.AspNetCore.Identity;
using BusinessObject.Notifications;
using BusinessObject.Branches;
using BusinessObject.Customers;
namespace BusinessObject.Authentication
{
    public class ApplicationUser : IdentityUser
    {
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

        // Legacy properties
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public string? Avatar { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public DateTime? LastPasswordChangeDate { get; set; }

        public IEnumerable<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();
        public virtual IEnumerable<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual Technician.Technician Technician { get; set; }
        //repair request 
        public virtual ICollection<RepairRequest> RepairRequests { get; set; } = new List<RepairRequest>();


        // --------------------------
        // Branch relationship
        public Guid? BranchId { get; set; }  // Nullable nếu user chưa được gán chi nhánh
        public virtual Branch Branch { get; set; }
    }

}
