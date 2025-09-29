using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Policies
{
    public class SecurityPolicyResponse
    {
        public int MinPasswordLength { get; set; }
        public bool RequireSpecialChar { get; set; }
        public bool RequireNumber { get; set; }
        public bool RequireUppercase { get; set; }
        public int SessionTimeout { get; set; }
        public int MaxLoginAttempts { get; set; }
        public int AccountLockoutTime { get; set; }
        public bool MfaRequired { get; set; }
        public int PasswordExpiryDays { get; set; }
        public bool EnableBruteForceProtection { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
