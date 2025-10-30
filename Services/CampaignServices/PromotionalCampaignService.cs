﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.Campaigns;
using Dtos.Campaigns;
using Microsoft.EntityFrameworkCore;
using Repositories.CampaignRepositories;
using Repositories.ServiceRepositories;

namespace Services.CampaignServices
{
    public class PromotionalCampaignService : IPromotionalCampaignService
    {
        private readonly IPromotionalCampaignRepository _repository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IMapper _mapper;

        public PromotionalCampaignService(IPromotionalCampaignRepository repository, IMapper mapper , IServiceRepository serviceRepository)
        {
            _repository = repository;
            _serviceRepository = serviceRepository;
            _mapper = mapper;
        }
        public async Task<(IEnumerable<PromotionalCampaignDto> Campaigns, int TotalCount)>
         GetPagedAsync(int page, int limit, string? search, CampaignType? type, bool? isActive, DateTime? startDate, DateTime? endDate)
        {
            var query = _repository.Query();

            // Filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.Contains(search) ||
                                         (c.Description != null && c.Description.Contains(search)));
            }

            if (type.HasValue)
            {
                query = query.Where(c => c.Type == type.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate.Value);
            }

            // Count tổng
            var totalCount = await query.CountAsync();

            // Paging
            var campaigns = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (_mapper.Map<IEnumerable<PromotionalCampaignDto>>(campaigns), totalCount);
        }
        public async Task<bool> UpdateStatusAsync(Guid id, bool isActive)
        {
            var campaign = await _repository.GetByIdAsync(id);
            if (campaign == null)
                return false;

            campaign.IsActive = isActive;
            campaign.UpdatedAt = DateTime.UtcNow;

            _repository.Update(campaign);
            await _repository.SaveChangesAsync();

            return true;
        }

            public async Task<bool> ActivateAsync(Guid id)
            {
                var campaign = await _repository.Query()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (campaign == null)
                    throw new KeyNotFoundException("Campaign not found.");

                // 🔹 Validate: không được kích hoạt nếu đã hết hạn
                if (campaign.EndDate.Date < DateTime.Today)
                    throw new InvalidOperationException("Cannot activate a campaign that has already expired.");

                // 🔹 Validate: không được kích hoạt nếu đã hết lượt sử dụng
                if (campaign.UsageLimit.HasValue && campaign.VoucherUsages.Count >= campaign.UsageLimit)
                    throw new InvalidOperationException("Cannot activate a campaign that has reached its usage limit.");

                // ✅ Hợp lệ → gọi repo để cập nhật
                await _repository.UpdateStatusAsync(id, true);
                await _repository.SaveChangesAsync();

                return true;
            }

        public async Task<bool> DeactivateAsync(Guid id)
            {
                var campaign = await _repository.Query()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (campaign == null)
                    throw new KeyNotFoundException("Campaign not found.");

                campaign.IsActive = false;
                await _repository.UpdateStatusAsync(id,false);
                await _repository.SaveChangesAsync();

                return true;
            }

        public async Task<bool> BulkUpdateStatusAsync(IEnumerable<Guid> ids, bool isActive)
        {
            var campaigns = await _repository.Query()
                .Where(pc => ids.Contains(pc.Id))
                .ToListAsync();

            if (!campaigns.Any())
                throw new KeyNotFoundException("No campaigns found for the specified IDs.");

            if (isActive)
            {
                var invalid = campaigns.Where(c =>
                    c.EndDate.Date < DateTime.Today ||
                    (c.UsageLimit.HasValue && c.UsedCount >= c.UsageLimit)
                ).ToList();

                if (invalid.Any())
                {
                    var names = string.Join(", ", invalid.Select(c => c.Name));
                    throw new InvalidOperationException($"Cannot activate expired or fully used campaigns: {names}");
                }
            }

            // Chỉ cập nhật những campaign hợp lệ
            var validIds = campaigns.Where(c =>
                !isActive || (
                    c.EndDate.Date >= DateTime.Today &&
                    (!c.UsageLimit.HasValue || c.UsedCount < c.UsageLimit)
                )).Select(c => c.Id);

            await _repository.UpdateStatusRangeAsync(validIds, isActive);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<PromotionalCampaignDto>> GetAllAsync()
        {
            var campaigns = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<PromotionalCampaignDto>>(campaigns);
        }

        public async Task<PromotionalCampaignDto?> GetByIdAsync(Guid id)
        {
            var campaign = await _repository.GetWithServicesAsync(id);
            return campaign is null ? null : _mapper.Map<PromotionalCampaignDto>(campaign);
        }

        public async Task<PromotionalCampaignDto> CreateAsync(CreatePromotionalCampaignDto dto)
        {
            await ValidateCreateAsync(dto);

            var campaign = _mapper.Map<PromotionalCampaign>(dto);
            campaign.Id = Guid.NewGuid();
            campaign.CreatedAt = DateTime.UtcNow;
            campaign.UpdatedAt = DateTime.UtcNow;

            foreach (var sid in dto.ServiceIds)
            {
                campaign.PromotionalCampaignServices.Add(new BusinessObject.Campaigns.PromotionalCampaignService
                {
                    PromotionalCampaignId = campaign.Id,
                    ServiceId = sid
                });
            }

            await _repository.AddAsync(campaign);
            await _repository.SaveChangesAsync();

            var createdCampaign = await _repository.GetWithServicesAsync(campaign.Id);
            return _mapper.Map<PromotionalCampaignDto>(createdCampaign);
        }




        public async Task<PromotionalCampaignDto?> UpdateAsync(UpdatePromotionalCampaignDto dto)
        {
            var campaign = await _repository.GetWithServicesAsync(dto.Id);
            if (campaign == null)
                return null;

            // Validate logic
            await ValidateUpdateAsync(dto, campaign);

            // Map changes
            _mapper.Map(dto, campaign);
            campaign.UpdatedAt = DateTime.UtcNow;

            // Update related services
            campaign.PromotionalCampaignServices.Clear();
            var existingServiceIds = await _serviceRepository.Query()
                .Where(s => dto.ServiceIds.Contains(s.ServiceId))
                .Select(s => s.ServiceId)
                .ToListAsync();

            foreach (var sid in existingServiceIds)
            {
                campaign.PromotionalCampaignServices.Add(new BusinessObject.Campaigns.PromotionalCampaignService
                {
                    PromotionalCampaignId = campaign.Id,
                    ServiceId = sid
                });
            }

            _repository.Update(campaign);
            await _repository.SaveChangesAsync();

            var updatedCampaign = await _repository.GetWithServicesAsync(campaign.Id);
            return _mapper.Map<PromotionalCampaignDto>(updatedCampaign);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var campaign = await _repository.Query()
                .Include(c => c.VoucherUsages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null)
                throw new KeyNotFoundException("Campaign not found.");

            //  Không cho xoá nếu còn active
            if (campaign.IsActive)
                throw new InvalidOperationException("Cannot delete an active campaign.");

            //  Không cho xoá nếu đã có lượt sử dụng
            if (campaign.VoucherUsages != null && campaign.VoucherUsages.Any())
                throw new InvalidOperationException("Cannot delete a campaign that has been used.");

            _repository.Delete(campaign);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteRangeAsync(IEnumerable<Guid> ids)
        {
            var toDelete = await _repository.Query()
                .Include(c => c.VoucherUsages)
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            if (!toDelete.Any())
                throw new KeyNotFoundException("No campaigns found for the specified IDs.");

            //  Không cho xoá nếu còn Active
            var activeCampaigns = toDelete.Where(c => c.IsActive).ToList();
            if (activeCampaigns.Any())
            {
                var names = string.Join(", ", activeCampaigns.Select(c => c.Name));
                throw new InvalidOperationException($"Cannot delete active campaigns: {names}");
            }

            //  Không cho xoá nếu có lượt sử dụng
            var usedCampaigns = toDelete.Where(c => c.VoucherUsages != null && c.VoucherUsages.Any()).ToList();
            if (usedCampaigns.Any())
            {
                var names = string.Join(", ", usedCampaigns.Select(c => c.Name));
                throw new InvalidOperationException($"Cannot delete campaigns that have been used: {names}");
            }

            _repository.DeleteRange(toDelete);
            await _repository.SaveChangesAsync();

            return true;
        }



        private async Task ValidateCreateAsync(CreatePromotionalCampaignDto dto)
        {
            // 🔹 1. Kiểm tra giá trị giảm giá hợp lệ
            if (dto.DiscountValue <= 0)
                throw new ArgumentException("Discount value must be greater than 0.");

            if (dto.DiscountType == DiscountType.Percentage)
            {
                if (dto.DiscountValue > 100 || dto.DiscountValue == 0)
                    throw new ArgumentException("Percentage discount cannot exceed 100%.");
            }
            else if (dto.DiscountType == DiscountType.Fixed)
            {
                // Với giảm giá tiền mặt (VNĐ hoặc USD)
                if (dto.DiscountValue < 1000)
                    throw new ArgumentException("Fixed amount discount must be at least 1000 VND.");
            }
            if (dto.UsageLimit <=0)
                throw new ArgumentException("UsageLimit must be greater than 0.");
            // 🔹 2. Kiểm tra ngày bắt đầu và kết thúc
            if (dto.StartDate >= dto.EndDate)
                throw new ArgumentException("Start date must be earlier than end date.");

            if (dto.EndDate < DateTime.Today)
                throw new ArgumentException("End date cannot be in the past.");

            // 🔹 3. Kiểm tra trùng tên khuyến mãi còn hiệu lực
            bool nameExists = await _repository.Query()
                .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower()
                            && c.EndDate >= DateTime.Today);

            if (nameExists)
                throw new ArgumentException($"A campaign with name '{dto.Name}' is already active or upcoming.");

            // 🔹 4. Kiểm tra ServiceIds tồn tại
            var existingServiceIds = await _serviceRepository.Query()
                .Where(s => dto.ServiceIds.Contains(s.ServiceId))
                .Select(s => s.ServiceId)
                .ToListAsync();

            var notFoundIds = dto.ServiceIds.Except(existingServiceIds).ToList();
            if (notFoundIds.Any())
                throw new ArgumentException($"Some services not found: {string.Join(",", notFoundIds)}");
        }


        private async Task ValidateUpdateAsync(UpdatePromotionalCampaignDto dto, PromotionalCampaign campaign)
        {
            // 🔹 1. Validate cơ bản
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Campaign name is required.");

            if (dto.StartDate >= dto.EndDate)
                throw new ArgumentException("End date must be greater than start date.");

            if (dto.DiscountValue <= 0)
                throw new ArgumentException("Discount value must be greater than 0.");

            // 🔹 2. Validate loại giảm giá
            if (dto.DiscountType == DiscountType.Percentage)
            {
                if (dto.DiscountValue > 100 || dto.DiscountValue == 0)
                    throw new ArgumentException("Percentage discount cannot exceed 100%.");
            }
            else if (dto.DiscountType == DiscountType.Fixed)
            {
                if (dto.DiscountValue < 1000)
                    throw new ArgumentException("Fixed amount discount must be at least 1000 VND.");
            }
            if (dto.UsageLimit <= 0)
                throw new ArgumentException("UsageLimit must be greater than 0.");
            // 🔹 3. Không cho chỉnh sửa nếu campaign đã được sử dụng
            bool hasUsage = campaign.VoucherUsages != null && campaign.VoucherUsages.Count > 0;
            if (hasUsage)
                throw new InvalidOperationException("Cannot update a campaign that has already been used in an order.");

            // 🔹 4. Không cho chỉnh ngày bắt đầu về quá khứ (nếu khác với giá trị cũ)
            if (dto.StartDate.Date < DateTime.Today && dto.StartDate.Date != campaign.StartDate.Date)
                throw new ArgumentException("Start date cannot be changed to a past date.");

            // 🔹 5. Không cho bật lại campaign đã hết hạn
            if (dto.IsActive && dto.EndDate.Date < DateTime.Today)
                throw new ArgumentException("Cannot activate a campaign that has already expired.");

            // 🔹 6. Validate danh sách dịch vụ tồn tại
            var existingServiceIds = await _serviceRepository.Query()
                .Where(s => dto.ServiceIds.Contains(s.ServiceId))
                .Select(s => s.ServiceId)
                .ToListAsync();

            var notFoundIds = dto.ServiceIds.Except(existingServiceIds).ToList();
            if (notFoundIds.Any())
                throw new ArgumentException($"Some services not found: {string.Join(", ", notFoundIds)}");

            // 🔹 7. Không cho trùng tên với campaign khác còn hiệu lực
            bool nameExists = await _repository.Query()
                .AnyAsync(c =>
                    c.Id != dto.Id &&
                    c.Name.ToLower() == dto.Name.ToLower() &&
                    c.EndDate >= DateTime.Today);

            if (nameExists)
                throw new ArgumentException($"A campaign with name '{dto.Name}' is already active or not yet expired.");
        }



    }

}
