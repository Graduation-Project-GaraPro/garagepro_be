using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.SystemLogs;
using Microsoft.AspNetCore.Identity;

namespace BusinessObject.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Avatar { get; set; }
        public DateTime? dateOfBirth { get; set; }

        public IEnumerable<SystemLog> SystemLogs { get; set; }
        // Custom claims
        //public virtual ICollection<ApplicationUserClaim> Claims { get; set; }
    }
}
