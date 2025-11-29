using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.Authentication;
using BusinessObject.Policies;
using Dtos;
using Dtos.Policies;
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
        
       
        private readonly IMapper _mapper;
        private readonly ILogger<SecurityPolicyService> _logger;
        private const string CacheKey = "CurrentSecurityPolicy";

        public SecurityPolicyService(ISecurityPolicyRepository repo, IMemoryCache cache,
             ILogger<SecurityPolicyService> logger, IMapper mapper)
        {
            _repo = repo;
            _cache = cache;
           
            _logger = logger;
            _mapper = mapper;
            
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

            existing.PasswordExpiryDays = updatedPolicy.PasswordExpiryDays;
            existing.EnableBruteForceProtection = updatedPolicy.EnableBruteForceProtection;
            existing.UpdatedBy = adminId;
            existing.UpdatedAt = DateTime.Now;

            var newJson = JsonSerializer.Serialize(existing);

            // Lịch sử
            var history = new SecurityPolicyHistory
            {
                HistoryId = Guid.NewGuid(),
                PolicyId = existing.Id,
                ChangedBy = adminId,
                ChangedAt = DateTime.Now,
                ChangeSummary = summary ?? "Admin updated security policy",
                PreviousValues = previousJson,
                NewValues = newJson
            };

            await _repo.UpdateAsync(existing);
            await _repo.AddHistoryAsync(history);
            await _repo.SaveChangesAsync();

            // Xóa cache để áp dụng ngay
            _cache.Remove(CacheKey);

            

            // Kích hoạt event để áp dụng real-time
            await OnPolicyUpdatedAsync(existing);
        }

        public async Task<SecurityPolicyHistory> GetHistoryAsync(Guid historyId)
            => await _repo.GetHistoryAsync(historyId);


        public async Task<IEnumerable<SecurityPolicyHistory>> GetAllHistoryAsync()
            => await _repo.GetAllHistoryAsync();

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
            if (policy == null)
            {
                return new PasswordValidationResult
                {
                    IsValid = true,
                    Errors = new Dictionary<string, List<string>>()
                };
            }

            var result = new PasswordValidationResult
            {
                Errors = new Dictionary<string, List<string>>()
            };

            var errors = new List<string>();

            if (password.Length < policy.MinPasswordLength)
                errors.Add($"Password must be at least {policy.MinPasswordLength} characters");

            if (policy.RequireSpecialChar && !password.Any(ch => !char.IsLetterOrDigit(ch)))
                errors.Add("Password must contain at least one special character");

            if (policy.RequireNumber && !password.Any(char.IsDigit))
                errors.Add("Password must contain at least one number");

            if (policy.RequireUppercase && !password.Any(char.IsUpper))
                errors.Add("Password must contain at least one uppercase letter");

            // Cập nhật kết quả
            result.IsValid = errors.Count == 0;

            if (errors.Count > 0)
            {
                result.Errors["Password"] = errors;
            }

            return result;
        }

        public async Task<PaginatedResponse<AuditHistoryDto>> GetAuditHistoryAsync(
        int page, int pageSize, string? search, string? changedBy, DateTime? dateFrom, DateTime? dateTo)
        {
            var (items, totalCount) = await _repo.GetAuditHistoryAsync(page, pageSize, search, changedBy, dateFrom, dateTo);

            var dtos = items.Select(h => new AuditHistoryDto
            {
                HistoryId = h.HistoryId,
                PolicyId = h.PolicyId,
                Policy = h.Policy?.ToString(), // hoặc null
                ChangedBy = h.ChangedBy,
                ChangedByUser = h.ChangedByUser?.UserName,
                ChangedAt = h.ChangedAt,
                ChangeSummary = h.ChangeSummary,
                PreviousValues = h.PreviousValues,
                NewValues = h.NewValues
            }).ToList();

            return new PaginatedResponse<AuditHistoryDto>
            {
                Data = dtos,
                Total = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<SecurityPolicyDto> RevertToSnapshotAsync(Guid historyId, string userId)
        {
            await using var transaction = await _repo.BeginTransactionAsync();
            try
            {
                // Lấy thông tin user
              

                var history = await _repo.GetHistoryAsync(historyId);
                if (history == null)
                    throw new KeyNotFoundException("Audit history not found");

                var snapshot = JsonSerializer.Deserialize<SecurityPolicy>(history.NewValues);
                if (snapshot == null)
                    throw new InvalidOperationException("Snapshot values are invalid");

                var policy = await _repo.GetCurrentAsync();
                if (policy == null)
                    throw new KeyNotFoundException("Policy not found");
                var policyDto = _mapper.Map<SecurityPolicyDto>(policy);

                var oldValuesJson = JsonSerializer.Serialize(policyDto);

                // update policy
                policy.MinPasswordLength = snapshot.MinPasswordLength;
                policy.RequireSpecialChar = snapshot.RequireSpecialChar;
                policy.RequireNumber = snapshot.RequireNumber;
                policy.RequireUppercase = snapshot.RequireUppercase;
                policy.SessionTimeout = snapshot.SessionTimeout;
                policy.MaxLoginAttempts = snapshot.MaxLoginAttempts;
                policy.AccountLockoutTime = snapshot.AccountLockoutTime;
                //policy.MfaRequired = snapshot.MfaRequired;
                policy.PasswordExpiryDays = snapshot.PasswordExpiryDays;
                policy.EnableBruteForceProtection = snapshot.EnableBruteForceProtection;
                policy.UpdatedBy = userId; // Thêm user name
                policy.UpdatedAt = DateTime.Now;

                await _repo.UpdateAsync(policy);

                var newHistory = new SecurityPolicyHistory
                {
                    HistoryId = Guid.NewGuid(),
                    PolicyId = policy.Id,
                    ChangedBy = userId, // Thêm user name
                    ChangedAt = DateTime.Now,
                    ChangeSummary = $"Reverted to snapshot at {history.ChangedAt} ",
                    PreviousValues = oldValuesJson,
                    NewValues = history.NewValues
                };
                await _repo.AddHistoryAsync(newHistory);

                await _repo.SaveChangesAsync();

                // Xóa cache để áp dụng ngay
                _cache.Remove(CacheKey);
                await transaction.CommitAsync();

                return _mapper.Map<SecurityPolicyDto>(policy);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<SecurityPolicyDto> UndoChangeAsync(Guid historyId, string userId)
        {
            await using var transaction = await _repo.BeginTransactionAsync();
            try
            {
                // Lấy thông tin user
              

                var history = await _repo.GetHistoryAsync(historyId);
                if (history == null)
                    throw new KeyNotFoundException("Audit history not found");

                var prevValues = JsonSerializer.Deserialize<SecurityPolicy>(history.PreviousValues);
                if (prevValues == null)
                    throw new InvalidOperationException("Previous values are invalid");

                var policy = await _repo.GetCurrentAsync();
                if (policy == null)
                    throw new KeyNotFoundException("Policy not found");
                var policyDto = _mapper.Map<SecurityPolicyDto>(policy);

                // lưu lại trạng thái trước khi undo
                var oldValuesJson = JsonSerializer.Serialize(policyDto);

                // cập nhật theo previous values
                policy.MinPasswordLength = prevValues.MinPasswordLength;
                policy.RequireSpecialChar = prevValues.RequireSpecialChar;
                policy.RequireNumber = prevValues.RequireNumber;
                policy.RequireUppercase = prevValues.RequireUppercase;
                policy.SessionTimeout = prevValues.SessionTimeout;
                policy.MaxLoginAttempts = prevValues.MaxLoginAttempts;
                policy.AccountLockoutTime = prevValues.AccountLockoutTime;
                //policy.MfaRequired = prevValues.MfaRequired;
                policy.PasswordExpiryDays = prevValues.PasswordExpiryDays;
                policy.EnableBruteForceProtection = prevValues.EnableBruteForceProtection;
                policy.UpdatedBy = userId; // Thêm user name
                policy.UpdatedAt = DateTime.Now;

                await _repo.UpdateAsync(policy);

                var newHistory = new SecurityPolicyHistory
                {
                    HistoryId = Guid.NewGuid(),
                    PolicyId = policy.Id,
                    ChangedBy = userId, // Thêm user name
                    ChangedAt = DateTime.Now,
                    ChangeSummary = $"Undid change made at {history.ChangedAt} ",
                    PreviousValues = oldValuesJson,
                    NewValues = history.PreviousValues
                };
                await _repo.AddHistoryAsync(newHistory);

                await _repo.SaveChangesAsync();

                // Xóa cache để áp dụng ngay
                _cache.Remove(CacheKey);

                await transaction.CommitAsync();

                return _mapper.Map<SecurityPolicyDto>(policy);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

        public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; } = new();
    }
}
