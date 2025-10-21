using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject;
using BusinessObject.Campaigns;
using Dtos.Branches;
using Dtos.Parts;
using Dtos.Services;
using Microsoft.EntityFrameworkCore;
using Repositories.ServiceRepositories;

namespace Services.ServiceServices
{
    public class ServiceCategoryService : IServiceCategoryService
    {
        private readonly IServiceCategoryRepository _repository;
        private readonly IMapper _mapper;

        public ServiceCategoryService(IServiceCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ServiceCategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ServiceCategoryDto>>(categories);
        }
        public async Task<IEnumerable<ServiceCategoryForBooking>> GetAllCategoriesForBookingAsync()
        {
            // Lấy tất cả categories + services + part + promotional campaigns
            var categories = await _repository.GetAllAsync();

            var result = categories.Select(cat => new ServiceCategoryForBooking
            {
                ServiceCategoryId = cat.ServiceCategoryId,
                CategoryName = cat.CategoryName,
                Services = cat.Services.Select(service => new ServiceDtoForBooking
                {
                    ServiceId = service.ServiceId,
                    ServiceCategoryId = service.ServiceCategoryId,
                    ServiceName = service.ServiceName,
                    Description = service.Description,
                    Price = service.Price,
                    DiscountedPrice = CalculateDiscountedPrice(service),
                    EstimatedDuration = service.EstimatedDuration,
                    IsActive = service.IsActive,
                    IsAdvanced = service.IsAdvanced,
                    CreatedAt = service.CreatedAt,
                    UpdatedAt = service.UpdatedAt,
                    ServiceCategory = new GetCategoryForServiceDto
                    {
                        ServiceCategoryId = service.ServiceCategory.ServiceCategoryId,
                        CategoryName = service.ServiceCategory.CategoryName
                    },
                    PartCategories = service.ServiceParts
                        .Where(sp => sp.Part != null)
                        .GroupBy(sp => sp.Part!.PartCategoryId)
                        .Select(g => new PartCategoryForBooking
                        {
                            PartCategoryId = g.Key,
                            CategoryName = g.First().Part!.PartCategory.CategoryName,
                            Parts = g.Select(sp => new PartDto
                            {
                                PartId = sp.PartId,
                                Name = sp.Part!.Name,
                                Price = sp.Part.Price
                            }).ToList()
                        }).ToList()
                }).ToList()
            }).ToList();

            return result;
        }


        // Hàm helper tính giá sau ưu đãi
        private decimal CalculateDiscountedPrice(Service service)
        {
            decimal price = service.Price;

            var activeCampaigns = service.PromotionalCampaignServices
                .Where(pcs => pcs.PromotionalCampaign.IsActive &&
                              pcs.PromotionalCampaign.StartDate <= DateTime.UtcNow &&
                              pcs.PromotionalCampaign.EndDate >= DateTime.UtcNow)
                .Select(pcs => pcs.PromotionalCampaign)
                .ToList();

            foreach (var campaign in activeCampaigns)
            {
                switch (campaign.DiscountType)
                {
                    case DiscountType.Percentage:
                        price -= price * campaign.DiscountValue / 100;
                        break;
                    case DiscountType.Fixed:
                        price -= campaign.DiscountValue;
                        break;
                    case DiscountType.FreeService:
                        price = 0;
                        break;
                }
            }

            return Math.Max(price, 0);
        }

        public async Task<ServiceCategoryDto> GetCategoryByIdAsync(Guid id)
        {
            var category = await _repository.GetByIdAsync(id);
            return _mapper.Map<ServiceCategoryDto>(category);
        }

        public async Task<IEnumerable<Dtos.Branches.ServiceDto>> GetServicesByCategoryIdAsync(Guid categoryId)
        {
            var services = await _repository.GetServicesByCategoryIdAsync(categoryId);
            return _mapper.Map<IEnumerable<Dtos.Branches.ServiceDto>>(services);
        }
        public async Task<ServiceCategoryDto> CreateCategoryAsync(CreateServiceCategoryDto dto)
        {
            // Check trùng tên trong cùng ServiceTypeId
            var exists = await _repository.Query()
                .AnyAsync(sc =>
                    sc.CategoryName.ToLower() == dto.CategoryName.ToLower()
                    );

            if (exists)
                throw new ApplicationException($"Category name '{dto.CategoryName}' already exists for this service type.");

            var entity = _mapper.Map<ServiceCategory>(dto);
            entity.ServiceCategoryId = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;

            _repository.Add(entity);
            await _repository.SaveChangesAsync();

            return _mapper.Map<ServiceCategoryDto>(entity);
        }

        public async Task<ServiceCategoryDto> UpdateCategoryAsync(Guid id, UpdateServiceCategoryDto dto)
        {
            var existing = await _repository.Query()
                .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == id);

            if (existing == null) return null;

            // Check trùng tên (ngoại trừ chính nó)
            var exists = await _repository.Query()
                .AnyAsync(sc =>
                    sc.CategoryName.ToLower() == dto.CategoryName.ToLower()             
                    && sc.ServiceCategoryId != id);

            if (exists)
                throw new ApplicationException($"Category name '{dto.CategoryName}' already exists for this service type.");

            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            _repository.Update(existing);
            await _repository.SaveChangesAsync();

            return _mapper.Map<ServiceCategoryDto>(existing);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            _repository.Delete(existing);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
