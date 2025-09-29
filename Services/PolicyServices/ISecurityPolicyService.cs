using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Policies;

namespace Services.PolicyServices
{
    public interface ISecurityPolicyService
    {
        Task<SecurityPolicy?> GetCurrentAsync();
        Task UpdatePolicyAsync(SecurityPolicy updatedPolicy, string adminId, string? summary = null);
        Task<PasswordValidationResult> ValidatePasswordAsync(string password);
        
    }
}
