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
                .FirstOrDefaultAsync(pc => pc.Id == id);
        }

        public async Task<PromotionalCampaign?> GetWithServicesAsync(Guid id)
        {
            return await _context.PromotionalCampaigns
                .Include(pc => pc.PromotionalCampaignServices)
                .ThenInclude(pcs => pcs.Service)
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

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
