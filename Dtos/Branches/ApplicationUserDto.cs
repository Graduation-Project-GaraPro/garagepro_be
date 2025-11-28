using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Branches
{
    public class ApplicationUserDto
    {
        public string Id { get; set; }             // từ IdentityUser
        public string UserName { get; set; }
        public string Email { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        // Computed property for FullName based on FirstName and LastName
        public string FullName => $"{FirstName} {LastName}".Trim();

        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public bool Gender { get; set; }           // true = Male, false = Female
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Status { get; set; }

        // Legacy
        public string? Avatar { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? LastPasswordChangeDate { get; set; }
    }

}