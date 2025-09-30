using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessObject.Authentication;
using BusinessObject.Policies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Repositories.PolicyRepositories;

namespace Services.PolicyServices
{
    public class SecurityPolicyService : ISecurityPolicyService
    {
        private readonly ISecurityPolicyRepository _repo;
        private readonly IMemoryCache _cache;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly ILogger<SecurityPolicyService> _logger;
        private const string CacheKey = "CurrentSecurityPolicy";

        public SecurityPolicyService(ISecurityPolicyRepository repo, IMemoryCache cache,
            IPasswordHasher<ApplicationUser> passwordHasher, ILogger<SecurityPolicyService> logger)
        {
            _repo = repo;
            _cache = cache;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<SecurityPolicy?> GetCurrentAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out SecurityPolicy? policy))
            {
                policy = await _repo.GetCurrentAsync();
                if (policy != null)
                {
                    _cache.Set(CacheKey, policy, TimeSpan.FromMinutes(1)); // Cache ngắn để real-time
                }
            }
            return policy;
        }

        public async Task UpdatePolicyAsync(SecurityPolicy updatedPolicy, string adminId, string? summary = null)
        {
            var existing = await _repo.GetCurrentAsync();
            if (existing == null)
                throw new InvalidOperationException("No existing security policy found.");

            // Validate tất cả giá trị
            ValidatePolicy(updatedPolicy);

            var previousJson = JsonSerializer.Serialize(existing);

            // Cập nhật giá trị
            existing.MinPasswordLength = updatedPolicy.MinPasswordLength;
            existing.RequireSpecialChar = updatedPolicy.RequireSpecialChar;
            existing.RequireNumber = updatedPolicy.RequireNumber;
            existing.RequireUppercase = updatedPolicy.RequireUppercase;
            existing.SessionTimeout = updatedPolicy.SessionTimeout;
            existing.MaxLoginAttempts = updatedPolicy.MaxLoginAttempts;
            existing.AccountLockoutTime = updatedPolicy.AccountLockoutTime;
            existing.MfaRequired = updatedPolicy.MfaRequired;
            existing.PasswordExpiryDays = updatedPolicy.PasswordExpiryDays;
            existing.EnableBruteForceProtection = updatedPolicy.EnableBruteForceProtection;
            existing.UpdatedBy = adminId;
            existing.UpdatedAt = DateTime.UtcNow;

            var newJson = JsonSerializer.Serialize(existing);

            // Lịch sử
            var history = new SecurityPolicyHistory
            {
                HistoryId = Guid.NewGuid(),
                PolicyId = existing.Id,
                ChangedBy = adminId,
                ChangedAt = DateTime.UtcNow,
                ChangeSummary = summary ?? "Admin updated security policy",
                PreviousValues = previousJson,
                NewValues = newJson
            };

            await _repo.UpdateAsync(existing);
            await _repo.AddHistoryAsync(history);
            await _repo.SaveChangesAsync();

            // Xóa cache để áp dụng ngay
            _cache.Remove(CacheKey);

            _logger.LogInformation("Security policy updated by admin {AdminId}", adminId);

            // Kích hoạt event để áp dụng real-time
            await OnPolicyUpdatedAsync(existing);
        }

        private void ValidatePolicy(SecurityPolicy policy)
        {
            if (policy.MinPasswordLength < 1)
                throw new ArgumentException("Minimum password length must be at least 1");

            if (policy.SessionTimeout < 1)
                throw new ArgumentException("Session timeout must be at least 1 minute");

            if (policy.MaxLoginAttempts < 1)
                throw new ArgumentException("Max login attempts must be at least 1");

            if (policy.AccountLockoutTime < 0) // 0 = vô hiệu hóa lockout
                throw new ArgumentException("Account lockout time cannot be negative");

            if (policy.PasswordExpiryDays < 0) // 0 = vô hiệu hóa expiry
                throw new ArgumentException("Password expiry days cannot be negative");
        }

        private async Task OnPolicyUpdatedAsync(SecurityPolicy newPolicy)
        {
            // Có thể thêm các hành động real-time ở đây
            // Ví dụ: broadcast đến tất cả connected clients
            _logger.LogInformation("New security policy applied: {@Policy}", newPolicy);
        }

        // Phương thức để validate password theo policy hiện tại
        public async Task<PasswordValidationResult> ValidatePasswordAsync(string password)
        {
            var policy = await GetCurrentAsync();
            if (policy == null) return new PasswordValidationResult { IsValid = true };

            var errors = new List<string>();

            if (password.Length < policy.MinPasswordLength)
                errors.Add($"Password must be at least {policy.MinPasswordLength} characters");

            if (policy.RequireSpecialChar && !password.Any(ch => !char.IsLetterOrDigit(ch)))
                errors.Add("Password must contain at least one special character");

            if (policy.RequireNumber && !password.Any(char.IsDigit))
                errors.Add("Password must contain at least one number");

            if (policy.RequireUppercase && !password.Any(char.IsUpper))
                errors.Add("Password must contain at least one uppercase letter");

            return new PasswordValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }
    }

    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
