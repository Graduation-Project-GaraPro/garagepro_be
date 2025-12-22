using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;

namespace BusinessObject.Policies
{
    public class SecurityPolicy
    {
        public Guid Id { get; set; } 

        public int MinPasswordLength { get; set; }
        public bool RequireSpecialChar { get; set; }
        public bool RequireNumber { get; set; }
        public bool RequireUppercase { get; set; }

        public int SessionTimeout { get; set; } 
        public int MaxLoginAttempts { get; set; }
        public int AccountLockoutTime { get; set; } 
        
        public int PasswordExpiryDays { get; set; }
        public bool EnableBruteForceProtection { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // FK đến bảng Users (người cập nhật gần nhất)
        public string? UpdatedBy { get; set; }
        public virtual ApplicationUser? UpdatedByUser { get; set; }

        // Quan hệ 1-n với lịch sử thay đổi
        public virtual ICollection<SecurityPolicyHistory>? Histories { get; set; } = new List<SecurityPolicyHistory>();

    }
}
