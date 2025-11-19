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


        // Hàm lấy ưu đãi tốt nhất cho một Service
        public PromotionalCampaign? GetBestPromotionForService(Guid serviceId, decimal orderValue = 0)
        {
            var now = DateTime.Now;

            var applicablePromotions = Query()
                .Where(pc => pc.IsActive &&
                            pc.StartDate <= now &&
                            pc.EndDate >= now &&
                            (pc.UsageLimit == null || pc.UsedCount < pc.UsageLimit) &&
                            pc.PromotionalCampaignServices.Any(pcs => pcs.ServiceId == serviceId) &&
                            (pc.MinimumOrderValue == null || orderValue >= pc.MinimumOrderValue))
                .ToList();

            if (!applicablePromotions.Any())
                return null;

            // Tính toán giá trị chiết khấu thực tế và chọn ưu đãi tốt nhất
            var bestPromotion = applicablePromotions
                .OrderByDescending(pc => CalculateActualDiscountValue(pc, orderValue))
                .ThenByDescending(pc => pc.DiscountValue)
                .FirstOrDefault();

            return bestPromotion;
        }

        // Hàm lấy tất cả ưu đãi khả dụng cho một Service (có thể dùng cho dropdown)
        public List<PromotionalCampaign> GetAvailablePromotionsForService(Guid serviceId)
        {
            var now = DateTime.Now;

            return Query()
                .Where(pc => pc.IsActive &&
                            pc.StartDate <= now &&
                            pc.EndDate >= now &&
                            (pc.UsageLimit == null || pc.UsedCount < pc.UsageLimit) &&
                            pc.PromotionalCampaignServices.Any(pcs => pcs.ServiceId == serviceId))
                .ToList();
        }

        // Hàm tính giá trị chiết khấu thực tế
        public decimal CalculateActualDiscountValue(PromotionalCampaign promotion, decimal orderValue)
        {
            if (promotion.DiscountType == DiscountType.Fixed)
            {
                // Với chiết khấu cố định
                if (promotion.MaximumDiscount.HasValue)
                {
                    return Math.Min(promotion.DiscountValue, promotion.MaximumDiscount.Value);
                }
                return promotion.DiscountValue;
            }
            else // Percentage
            {
                // Với chiết khấu phần trăm
                var discountAmount = orderValue * promotion.DiscountValue / 100;

                if (promotion.MaximumDiscount.HasValue)
                {
                    return Math.Min(discountAmount, promotion.MaximumDiscount.Value);
                }
                return discountAmount;
            }
        }
        public bool IsPromotionApplicableForService(Guid promotionId, Guid serviceId, decimal orderValue = 0)
        {
            var now = DateTime.Now;

            return Query()
                .Any(pc => pc.Id == promotionId &&
                          pc.IsActive &&
                          pc.StartDate <= now &&
                          pc.EndDate >= now &&
                          (pc.UsageLimit == null || pc.UsedCount < pc.UsageLimit) &&
                          pc.PromotionalCampaignServices.Any(pcs => pcs.ServiceId == serviceId) &&
                          (pc.MinimumOrderValue == null || orderValue >= pc.MinimumOrderValue));
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


        public async Task<bool> ExistsAsync(Guid id)
        {
            var campaignExist = await _context.PromotionalCampaigns
                .AnyAsync(pc => pc.Id == id);
                
            return campaignExist;
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        
    }
}
