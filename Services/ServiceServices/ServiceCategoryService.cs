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

        public async Task<IEnumerable<ServiceCategoryDto>> GetAllCategoriesWithFilterAsync(
           Guid? parentServiceCategoryId = null,
           string? searchTerm = null,
           bool? isActive = null)
        {
            // 🔹 Gọi đến repository
            var categories = await _repository.GetAllCategoriesWithFilterAsync(
                parentServiceCategoryId, searchTerm, isActive);

            // 🔹 Map sang DTO
            var categoryDtos = _mapper.Map<IEnumerable<ServiceCategoryDto>>(categories);

            return categoryDtos;
        }
        public async Task<object> GetAllCategoriesForBookingAsync(
                    int pageNumber = 1,
                    int pageSize = 10,
                    Guid? serviceCategoryId = null,
                    string? searchTerm = null)
        {
            var (categories, totalCount) = await _repository.GetCategoriesForBookingAsync(pageNumber, pageSize, serviceCategoryId, searchTerm);

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

            return new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = result
            };
        }


        public async Task<object> GetAllServiceCategoryFromParentCategoryAsync(
            Guid parentServiceCategoryId,
            int pageNumber = 1,
            int pageSize = 10,
            Guid? childServiceCategoryId = null,
            string? searchTerm = null)
        {
            var (categories, totalCount) = await _repository.GetCategoriesByParentAsync(
                parentServiceCategoryId,
                pageNumber,
                pageSize,
                childServiceCategoryId,
                searchTerm
            );

            var result = categories.Select(cat => new ServiceCategoryForBooking
            {
                ServiceCategoryId = cat.ServiceCategoryId,
                ParentServiceCategoryId = cat.ParentServiceCategoryId,
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

            return new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = result
            };
        }

        public async Task<IEnumerable<ServiceCategoryDto>> GetParentCategoriesAsync()
        {
            var parentCategories = await _repository.GetParentCategoriesAsync();

            return parentCategories.Select(cat => new ServiceCategoryDto
            {
                ServiceCategoryId = cat.ServiceCategoryId,
                CategoryName = cat.CategoryName,
               
                ParentServiceCategoryId = cat.ParentServiceCategoryId,
                Description = cat.Description,
                IsActive = cat.IsActive,
                CreatedAt = cat.CreatedAt,
                UpdatedAt = cat.UpdatedAt,
                Services = cat.Services?.Select(s => new Dtos.Services.ServiceDto        {
                    ServiceId = s.ServiceId,
                    ServiceCategoryId = s.ServiceCategoryId,
                    ServiceName = s.ServiceName,
                    Description = s.Description,
                    Price = s.Price,
                    IsActive = s.IsActive
                }).ToList() ?? new List<Dtos.Services.ServiceDto>(),
                ChildCategories = cat.ChildServiceCategories?.Select(child => new ServiceCategoryDto
                {
                    ServiceCategoryId = child.ServiceCategoryId,
                    CategoryName = child.CategoryName,
                    ParentServiceCategoryId = child.ParentServiceCategoryId,
                    Description = child.Description,
                    IsActive = child.IsActive,
                    CreatedAt = child.CreatedAt,
                    UpdatedAt = child.UpdatedAt
                }).ToList() ?? new List<ServiceCategoryDto>()
            }).ToList();
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
            //  Check trùng tên
            var exists = await _repository.Query()
                .AnyAsync(sc => sc.CategoryName.ToLower() == dto.CategoryName.ToLower());

            if (exists)
                throw new ApplicationException($"Category name '{dto.CategoryName}' already exists.");

            //  Kiểm tra logic danh mục cha (chỉ cho phép 2 cấp)
            if (dto.ParentServiceCategoryId.HasValue)
            {
                var parent = await _repository.Query()
                    .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == dto.ParentServiceCategoryId.Value);

                if (parent == null)
                    throw new ApplicationException("Parent category does not exist.");

                //  Không cho phép chọn cha mà bản thân nó cũng là con (tức cấp 2)
                if (parent.ParentServiceCategoryId != null)
                    throw new ApplicationException("Cannot assign a subcategory as a parent. Only 2 levels allowed.");

                //  Không cho phép parent đang có dịch vụ
                if (parent.Services != null && parent.Services.Any())
                    throw new ApplicationException("A parent category cannot directly contain services.");
            }

            //  Tạo mới
            var entity = _mapper.Map<ServiceCategory>(dto);
            entity.ServiceCategoryId = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;

            _repository.Add(entity);
            await _repository.SaveChangesAsync();

            return _mapper.Map<ServiceCategoryDto>(entity);
        }



        public async Task<IEnumerable<ServiceCategoryDto>> GetValidParentCategoriesAsync(Guid? categoryId)
        {
            var allCategories = await _repository.GetAllAsync();

            ServiceCategory? currentCategory = null;
            if (categoryId != null && categoryId != Guid.Empty)
            {
                currentCategory = await _repository.GetByIdAsync(categoryId.Value);

                if (currentCategory == null)
                    throw new ApplicationException("Category not found.");
            }

            // 1 Nếu đang tạo mới (chưa có categoryId)
            if (currentCategory == null)
            {
                // Chỉ cho phép chọn cha cấp 1 hợp lệ (không có cha, không chứa dịch vụ)
                var validParents = allCategories
                    .Where(c => c.ParentServiceCategoryId == null &&
                                (c.Services == null || !c.Services.Any()))
                    .ToList();

                return _mapper.Map<IEnumerable<ServiceCategoryDto>>(validParents);
            }

            // 2 Nếu là CHA (cấp 1) => không thể có cha khác
            if (currentCategory.ParentServiceCategoryId == null)
            {
                // Nhưng nếu nó đang có con thì tuyệt đối không cho gán cha
                if (allCategories.Any(c => c.ParentServiceCategoryId == currentCategory.ServiceCategoryId))
                {
                    return Enumerable.Empty<ServiceCategoryDto>();
                }

                // Nếu không có con, thì vẫn có thể gán cha (tức chuyển nó thành con)
                // chỉ cho phép chọn cha là cấp 1 khác
                var validParents = allCategories
                    .Where(c =>
                        c.ServiceCategoryId != categoryId &&
                        c.ParentServiceCategoryId == null && // là cấp 1
                        (c.Services == null || !c.Services.Any()))
                    .ToList();

                return _mapper.Map<IEnumerable<ServiceCategoryDto>>(validParents);
            }

            //  Nếu là CON (cấp 2) => chỉ có thể chọn cha là cấp 1 khác
            var validParentsForChild = allCategories
                .Where(c =>
                    c.ServiceCategoryId != categoryId &&
                    c.ParentServiceCategoryId == null &&
                    (c.Services == null || !c.Services.Any()))
                .ToList();

            return _mapper.Map<IEnumerable<ServiceCategoryDto>>(validParentsForChild);
        }


        //  Hàm đệ quy để lấy tất cả danh mục con của 1 danh mục
        private IEnumerable<ServiceCategory> GetAllDescendants(ServiceCategory category, List<ServiceCategory> all)
        {
            var children = all.Where(c => c.ParentServiceCategoryId == category.ServiceCategoryId).ToList();
            foreach (var child in children)
            {
                yield return child;
                foreach (var descendant in GetAllDescendants(child, all))
                {
                    yield return descendant;
                }
            }
        }


        public async Task<IEnumerable<ServiceCategoryDto>> GetParentCategoriesForFilterAsync()
        {
            var query = _repository.Query().Where(c => c.ParentServiceCategoryId == null);          
            var categories = await query .OrderBy(c => c.CategoryName) .ToListAsync(); 
            return _mapper.Map<IEnumerable<ServiceCategoryDto>>(categories); 
        }


        public async Task<ServiceCategoryDto> UpdateCategoryAsync(Guid id, UpdateServiceCategoryDto dto)
        {
            var existing = await _repository.Query()
                .Include(c => c.ChildServiceCategories)
                .Include(c => c.Services)
                .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == id);

            if (existing == null)
                throw new ApplicationException("Category not found.");

            //  Check trùng tên (ngoại trừ chính nó)
            var exists = await _repository.Query()
                .AnyAsync(sc =>
                    sc.CategoryName.ToLower() == dto.CategoryName.ToLower()
                    && sc.ServiceCategoryId != id);

            if (exists)
                throw new ApplicationException($"Category name '{dto.CategoryName}' already exists.");

            //  Nếu là CHA có con => không được có Service
            if (existing.ChildServiceCategories.Any() && existing.Services.Any())
            {
                throw new ApplicationException("A parent category cannot directly contain services.");
            }

            //  Kiểm tra danh mục cha (nếu có)
            if (dto.ParentServiceCategoryId != null)
            {
                if (dto.ParentServiceCategoryId == id)
                    throw new ApplicationException("A category cannot be its own parent.");

                var parent = await _repository.Query()
                    .FirstOrDefaultAsync(p => p.ServiceCategoryId == dto.ParentServiceCategoryId.Value);

                if (parent == null)
                    throw new ApplicationException("Parent category not found.");

                //  Không cho phép cha là con (chỉ 2 cấp)
                if (parent.ParentServiceCategoryId != null)
                    throw new ApplicationException("Cannot assign a subcategory as a parent. Only 2 levels allowed.");

                //  Không cho phép cha đang có dịch vụ
                if (parent.Services != null && parent.Services.Any())
                    throw new ApplicationException("A parent category cannot directly contain services.");
            }

            //  Nếu danh mục hiện tại đang là CHA có con => không được đổi thành con
            if (existing.ChildServiceCategories.Any() && dto.ParentServiceCategoryId != null)
            {
                throw new ApplicationException("A parent category cannot be assigned under another parent.");
            }

            //  Áp dụng update
            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            _repository.Update(existing);
            await _repository.SaveChangesAsync();

            return _mapper.Map<ServiceCategoryDto>(existing);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var existing = await _repository.Query()
                .Include(c => c.ChildServiceCategories)
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.ServiceCategoryId == id);

            if (existing == null)
                throw new ApplicationException("Category not found.");

            //  Nếu danh mục có con → không cho xóa
            if (existing.ChildServiceCategories != null && existing.ChildServiceCategories.Any())
                throw new ApplicationException("Cannot delete a parent category that has subcategories.");

            // Nếu danh mục có Service → không cho xóa
            if (existing.Services != null && existing.Services.Any())
                throw new ApplicationException("Cannot delete a category that contains services.");

           
            _repository.Delete(existing);
            await _repository.SaveChangesAsync();

            return true;
        }

    }
}
