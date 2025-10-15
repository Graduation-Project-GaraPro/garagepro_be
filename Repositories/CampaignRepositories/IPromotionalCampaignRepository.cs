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
        Task<IQueryable<PromotionalCampaign>> QueryAsync();
        Task<PromotionalCampaign?> GetByIdAsync(Guid id);
        Task<PromotionalCampaign?> GetWithServicesAsync(Guid id);

        Task UpdateStatusAsync(Guid id, bool isActive);

        Task UpdateStatusRangeAsync(IEnumerable<Guid> ids, bool isActive);

        void DeleteRange(IEnumerable<PromotionalCampaign> campaigns);
        Task AddAsync(PromotionalCampaign campaign);
        void Update(PromotionalCampaign campaign);
        void Delete(PromotionalCampaign campaign);
        Task SaveChangesAsync();
    }

}
