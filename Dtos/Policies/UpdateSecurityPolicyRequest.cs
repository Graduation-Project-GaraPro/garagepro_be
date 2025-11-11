using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Policies
{
    public class UpdateSecurityPolicyRequest
    {
        [Range(6, 32, ErrorMessage = "Password length must be between 6 and 32 characters")]
        public int MinPasswordLength { get; set; }

        public bool RequireSpecialChar { get; set; }
        public bool RequireNumber { get; set; }
        public bool RequireUppercase { get; set; }

        [Range(1, 1440, ErrorMessage = "Session timeout must be between 5 and 1440 minutes")]
        public int SessionTimeout { get; set; }

        [Range(1, 10, ErrorMessage = "Max login attempts must be between 1 and 10")]
        public int MaxLoginAttempts { get; set; }

        [Range(1, 1440, ErrorMessage = "Account lockout time must be between 1 and 1440 minutes")]
        public int AccountLockoutTime { get; set; }

        public bool MfaRequired { get; set; }

        [Range(1, 365, ErrorMessage = "Password expiry must be between 1 and 365 days")] public int PasswordExpiryDays { get; set; }

        public bool EnableBruteForceProtection { get; set; }

        [StringLength(500, ErrorMessage = "Change summary cannot exceed 500 characters")]
        public string? ChangeSummary { get; set; }
    }
}
