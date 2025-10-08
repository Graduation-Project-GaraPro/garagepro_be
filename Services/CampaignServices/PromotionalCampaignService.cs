using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.Campaigns;
using Dtos.Campaigns;
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
                .Where(s => dto.ServiceIds.Contains(s.Id))
                .Select(s => s.Id)
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
                .Where(s => dto.ServiceIds.Contains(s.Id))
                .Select(s => s.Id)
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
    }

}
