using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Policies;
using Dtos;
using Dtos.Policies;

namespace Services.PolicyServices
{
    public interface ISecurityPolicyService
    {
        Task<SecurityPolicy?> GetCurrentAsync();
        Task UpdatePolicyAsync(SecurityPolicy updatedPolicy, string adminId, string? summary = null);
        Task<SecurityPolicyHistory> GetHistoryAsync(Guid historyId);


        Task<SecurityPolicyDto> UndoChangeAsync(Guid historyId, string userId);

        Task<SecurityPolicyDto> RevertToSnapshotAsync(Guid historyId, string userId);


        Task<IEnumerable<SecurityPolicyHistory>> GetAllHistoryAsync();
        Task<PaginatedResponse<AuditHistoryDto>> GetAuditHistoryAsync(
        int page, int pageSize, string? search, string? changedBy, DateTime? dateFrom, DateTime? dateTo);
        Task<PasswordValidationResult> ValidatePasswordAsync(string password);
        
    }
}
