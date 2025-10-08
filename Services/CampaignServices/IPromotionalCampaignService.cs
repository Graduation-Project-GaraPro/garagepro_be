using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dtos.Campaigns;

namespace Services.CampaignServices
{
    public interface IPromotionalCampaignService
    {
        Task<IEnumerable<PromotionalCampaignDto>> GetAllAsync();
        Task<PromotionalCampaignDto?> GetByIdAsync(Guid id);
        Task<PromotionalCampaignDto> CreateAsync(CreatePromotionalCampaignDto dto);
        Task<PromotionalCampaignDto?> UpdateAsync(UpdatePromotionalCampaignDto dto);
        Task<bool> DeleteAsync(Guid id);
    }

}
