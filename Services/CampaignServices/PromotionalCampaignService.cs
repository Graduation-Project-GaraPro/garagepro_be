using System;
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
            var query = await _repository.QueryAsync();

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
            return await UpdateStatusAsync(id, true);
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            return await UpdateStatusAsync(id, false);
        }

        public async Task<bool> BulkUpdateStatusAsync(IEnumerable<Guid> ids, bool isActive)
        {
            var campaigns = await _repository.QueryAsync();
            var toUpdate = campaigns.Where(pc => ids.Contains(pc.Id)).ToList();

            if (!toUpdate.Any())
                return false;

            await _repository.UpdateStatusRangeAsync(ids, isActive);
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
            // validate ServiceIds
            var existingServiceIds = await _serviceRepository.Query()
                .Where(s => dto.ServiceIds.Contains(s.ServiceId)) 
                .Select(s => s.ServiceId)                      
                .ToListAsync();


            var notFoundIds = dto.ServiceIds.Except(existingServiceIds).ToList();
            if (notFoundIds.Any())
            {
                throw new ArgumentException($"Some services not found: {string.Join(",", notFoundIds)}");
            }

            var campaign = _mapper.Map<PromotionalCampaign>(dto);
            campaign.Id = Guid.NewGuid();
            campaign.CreatedAt = DateTime.UtcNow;
            campaign.UpdatedAt = DateTime.UtcNow;

            foreach (var sid in existingServiceIds)
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
            if (campaign == null) return null;

            // validate ServiceIds
            var existingServiceIds = await _serviceRepository.Query()
                 .Where(s => dto.ServiceIds.Contains(s.ServiceId)) 
                 .Select(s => s.ServiceId)                        
                 .ToListAsync();

            var notFoundIds = dto.ServiceIds.Except(existingServiceIds).ToList();
            if (notFoundIds.Any())
            {
                throw new ArgumentException($"Some services not found: {string.Join(",", notFoundIds)}");
            }

            _mapper.Map(dto, campaign);
            campaign.UpdatedAt = DateTime.UtcNow;

            // clear old relations
            campaign.PromotionalCampaignServices.Clear();

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
            var campaign = await _repository.GetByIdAsync(id);
            if (campaign == null) return false;

            _repository.Delete(campaign);
            await _repository.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteRangeAsync(IEnumerable<Guid> ids)
        {
            var campaigns = await _repository.QueryAsync();
            var toDelete = campaigns.Where(pc => ids.Contains(pc.Id)).ToList();

            if (!toDelete.Any())
                return false;

            // Rule: không cho xoá nếu còn active
            if (toDelete.Any(c => c.IsActive))
            {
                throw new InvalidOperationException("Cannot delete active campaigns.");
            }

            _repository.DeleteRange(toDelete);
            await _repository.SaveChangesAsync();

            return true;
        }

    }

}
