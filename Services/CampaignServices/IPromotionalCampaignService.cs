using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Campaigns;
using Dtos.Campaigns;

namespace Services.CampaignServices
{
    public interface IPromotionalCampaignService
    {
        Task<(IEnumerable<PromotionalCampaignDto> Campaigns, int TotalCount)>
         GetPagedAsync(int page, int limit, string? search, CampaignType? type, bool? isActive, DateTime? startDate, DateTime? endDate);

        Task<IEnumerable<PromotionalCampaignDto>> GetAllAsync();
        Task<bool> ActivateAsync(Guid id);
        Task<bool> DeactivateAsync(Guid id);
        Task<bool> BulkUpdateStatusAsync(IEnumerable<Guid> ids, bool isActive);
        Task<bool> UpdateStatusAsync(Guid id, bool isActive);
        Task<PromotionalCampaignDto?> GetByIdAsync(Guid id);
        Task<PromotionalCampaignDto> CreateAsync(CreatePromotionalCampaignDto dto);
        Task<PromotionalCampaignDto?> UpdateAsync(UpdatePromotionalCampaignDto dto);
        Task<bool> DeleteRangeAsync(IEnumerable<Guid> ids);
        Task<bool> DeleteAsync(Guid id);
    }

}
