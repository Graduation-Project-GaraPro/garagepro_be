using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Campaigns;

namespace Repositories.CampaignRepositories
{
    public interface IPromotionalCampaignRepository
    {
        Task<IEnumerable<PromotionalCampaign>> GetAllAsync();
        Task<PromotionalCampaign?> GetByIdAsync(Guid id);
        Task<PromotionalCampaign?> GetWithServicesAsync(Guid id);
        Task AddAsync(PromotionalCampaign campaign);
        void Update(PromotionalCampaign campaign);
        void Delete(PromotionalCampaign campaign);
        Task SaveChangesAsync();
    }

}
