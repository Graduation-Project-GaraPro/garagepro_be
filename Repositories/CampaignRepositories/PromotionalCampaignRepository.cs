using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Campaigns;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories.CampaignRepositories
{
    public class PromotionalCampaignRepository : IPromotionalCampaignRepository
    {
        private readonly MyAppDbContext _context;

        public PromotionalCampaignRepository(MyAppDbContext context)
        {
            _context = context;
        }
        public  IQueryable<PromotionalCampaign> Query()
        {
            return _context.PromotionalCampaigns
                .Include(pc => pc.PromotionalCampaignServices)
                .ThenInclude(pcs => pcs.Service)
                .Include(pc => pc.VoucherUsages).ThenInclude(v=>v.Customer)
                .Include(pc => pc.VoucherUsages).ThenInclude(v=>v.RepairOrder)
                .AsQueryable();
        }
        public async Task<IEnumerable<PromotionalCampaign>> GetAllAsync()
        {
            return await _context.PromotionalCampaigns
                .Include(pc => pc.PromotionalCampaignServices)
                .ThenInclude(pcs => pcs.Service)
                .ToListAsync();
        }

       

        public async Task<PromotionalCampaign?> GetByIdAsync(Guid id)
        {
            return await _context.PromotionalCampaigns
                .Include(pc => pc.PromotionalCampaignServices)
                .ThenInclude(pcs => pcs.Service)
                .Include(pc => pc.VoucherUsages)
                .FirstOrDefaultAsync(pc => pc.Id == id);
        }

        public async Task<PromotionalCampaign?> GetWithServicesAsync(Guid id)
        {
            return await _context.PromotionalCampaigns
                .Include(pc => pc.PromotionalCampaignServices)
                .ThenInclude(pcs => pcs.Service)
                 .Include(pc => pc.VoucherUsages)
                .FirstOrDefaultAsync(pc => pc.Id == id);
        }

        public async Task AddAsync(PromotionalCampaign campaign)
        {
            await _context.PromotionalCampaigns.AddAsync(campaign);
        }

        public void Update(PromotionalCampaign campaign)
        {
            _context.PromotionalCampaigns.Update(campaign);
        }

        public void Delete(PromotionalCampaign campaign)
        {
            _context.PromotionalCampaigns.Remove(campaign);
        }
        public void DeleteRange(IEnumerable<PromotionalCampaign> campaigns)
        {
            if (campaigns != null && campaigns.Any())
            {
                _context.PromotionalCampaigns.RemoveRange(campaigns);
            }
        }

        public async Task UpdateStatusAsync(Guid id, bool isActive)
        {
            var campaign = await _context.PromotionalCampaigns.FindAsync(id);
            if (campaign == null) return;

            campaign.IsActive = isActive;
            campaign.UpdatedAt = DateTime.UtcNow;

            _context.PromotionalCampaigns.Update(campaign);
        }
        public async Task UpdateStatusRangeAsync(IEnumerable<Guid> ids, bool isActive)
        {
            var campaigns = await _context.PromotionalCampaigns
                .Where(pc => ids.Contains(pc.Id))
                .ToListAsync();

            if (!campaigns.Any()) return;

            foreach (var campaign in campaigns)
            {
                campaign.IsActive = isActive;
                campaign.UpdatedAt = DateTime.UtcNow;
            }

            _context.PromotionalCampaigns.UpdateRange(campaigns);
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
