using AutoMapper;
using Dtos.Quotations;
using Microsoft.EntityFrameworkCore;
using Repositories.ServiceRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.QuotationServices
{
    public class QuotationTreeSelectionService : IQuotationTreeSelectionService
    {
        private readonly IServiceCategoryRepository _serviceCategoryRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IMapper _mapper;

        public QuotationTreeSelectionService(
            IServiceCategoryRepository serviceCategoryRepository,
            IServiceRepository serviceRepository,
            IMapper mapper)
        {
            _serviceCategoryRepository = serviceCategoryRepository;
            _serviceRepository = serviceRepository;
            _mapper = mapper;
        }

        public async Task<ServiceCategoryTreeResponseDto> GetRootCategoriesAsync()
        {
            // Get all root categories (categories with no parent)
            var rootCategories = await _serviceCategoryRepository.Query()
                .Where(sc => sc.ParentServiceCategoryId == null && sc.IsActive)
                .ToListAsync();

            var categoryNodes = new List<ServiceCategoryTreeNodeDto>();

            foreach (var category in rootCategories)
            {
                var childCount = await _serviceCategoryRepository.Query()
                    .CountAsync(sc => sc.ParentServiceCategoryId == category.ServiceCategoryId);

                var serviceCount = await _serviceRepository.Query()
                    .CountAsync(s => s.ServiceCategoryId == category.ServiceCategoryId && s.IsActive);

                categoryNodes.Add(new ServiceCategoryTreeNodeDto
                {
                    ServiceCategoryId = category.ServiceCategoryId,
                    CategoryName = category.CategoryName,
                    Description = category.Description,
                    ParentServiceCategoryId = null,
                    HasChildren = childCount > 0,
                    ServiceCount = serviceCount,
                    ChildCategoryCount = childCount
                });
            }

            return new ServiceCategoryTreeResponseDto
            {
                CurrentCategoryId = null,
                CurrentCategoryName = "Root",
                ChildCategories = categoryNodes,
                Services = new List<ServiceForSelectionDto>(),
                Breadcrumb = new BreadcrumbDto
                {
                    Items = new List<BreadcrumbItemDto>
                    {
                        new BreadcrumbItemDto
                        {
                            CategoryId = null,
                            CategoryName = "All Categories",
                            Level = 0
                        }
                    }
                }
            };
        }

        public async Task<ServiceCategoryTreeResponseDto> GetCategoryChildrenAsync(Guid categoryId)
        {
            var category = await _serviceCategoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new ArgumentException($"Service category with ID {categoryId} not found.");

            // Get child categories
            var childCategories = await _serviceCategoryRepository.Query()
                .Where(sc => sc.ParentServiceCategoryId == categoryId && sc.IsActive)
                .ToListAsync();

            var categoryNodes = new List<ServiceCategoryTreeNodeDto>();

            foreach (var child in childCategories)
            {
                var childCount = await _serviceCategoryRepository.Query()
                    .CountAsync(sc => sc.ParentServiceCategoryId == child.ServiceCategoryId);

                var serviceCount = await _serviceRepository.Query()
                    .CountAsync(s => s.ServiceCategoryId == child.ServiceCategoryId && s.IsActive);

                categoryNodes.Add(new ServiceCategoryTreeNodeDto
                {
                    ServiceCategoryId = child.ServiceCategoryId,
                    CategoryName = child.CategoryName,
                    Description = child.Description,
                    ParentServiceCategoryId = categoryId,
                    HasChildren = childCount > 0,
                    ServiceCount = serviceCount,
                    ChildCategoryCount = childCount
                });
            }

            // Get services directly under this category
            var services = await _serviceRepository.Query()
                .Include(s => s.ServiceCategory)
                .Where(s => s.ServiceCategoryId == categoryId && s.IsActive)
                .ToListAsync();

            var serviceDtos = services.Select(s => new ServiceForSelectionDto
            {
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName,
                Description = s.Description,
                Price = s.Price,
                EstimatedDuration = (double)s.EstimatedDuration,
                IsAdvanced = s.IsAdvanced,
                ServiceCategoryId = s.ServiceCategoryId,
                ServiceCategoryName = s.ServiceCategory?.CategoryName ?? ""
            }).ToList();

            // Build breadcrumb
            var breadcrumb = await BuildBreadcrumbAsync(categoryId);

            return new ServiceCategoryTreeResponseDto
            {
                CurrentCategoryId = categoryId,
                CurrentCategoryName = category.CategoryName,
                ChildCategories = categoryNodes,
                Services = serviceDtos,
                Breadcrumb = breadcrumb
            };
        }

        /// <summary>
        /// Build breadcrumb trail from root to current category
        /// </summary>
        private async Task<BreadcrumbDto> BuildBreadcrumbAsync(Guid categoryId)
        {
            var currentCategory = await _serviceCategoryRepository.GetByIdAsync(categoryId);
            var level = 0;

            // Build breadcrumb from current to root
            var trail = new List<BreadcrumbItemDto>();
            while (currentCategory != null)
            {
                trail.Insert(0, new BreadcrumbItemDto
                {
                    CategoryId = currentCategory.ServiceCategoryId,
                    CategoryName = currentCategory.CategoryName,
                    Level = level
                });

                if (currentCategory.ParentServiceCategoryId.HasValue)
                {
                    currentCategory = await _serviceCategoryRepository.GetByIdAsync(currentCategory.ParentServiceCategoryId.Value);
                    level++;
                }
                else
                {
                    break;
                }
            }

            // Add root
            trail.Insert(0, new BreadcrumbItemDto
            {
                CategoryId = null,
                CategoryName = "All Categories",
                Level = 0
            });

            // Renumber levels
            for (int i = 0; i < trail.Count; i++)
            {
                trail[i].Level = i;
            }

            return new BreadcrumbDto { Items = trail };
        }
    }
}
