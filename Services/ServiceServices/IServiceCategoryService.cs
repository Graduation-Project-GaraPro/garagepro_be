using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dtos.Branches;
using Dtos.Services;

namespace Services.ServiceServices
{
    public interface IServiceCategoryService
    {
        Task<IEnumerable<ServiceCategoryDto>> GetAllCategoriesAsync();
        Task<ServiceCategoryDto> GetCategoryByIdAsync(Guid id);
        Task<IEnumerable<Dtos.Branches.ServiceDto>> GetServicesByCategoryIdAsync(Guid categoryId);
        Task<object> GetAllCategoriesForBookingAsync(
            int pageNumber = 1,
            int pageSize = 10,
            Guid? serviceCategoryId = null,
            string? searchTerm = null);

        Task<IEnumerable<ServiceCategoryDto>> GetParentCategoriesForFilterAsync();

        Task<IEnumerable<ServiceCategoryDto>> GetAllCategoriesWithFilterAsync(
           Guid? parentServiceCategoryId = null,
           string? searchTerm = null,
           bool? isActive = null);
        Task<IEnumerable<ServiceCategoryDto>> GetValidParentCategoriesAsync(Guid? categoryId);
        Task<object> GetAllServiceCategoryFromParentCategoryAsync(
            Guid parentServiceCategoryId,
            int pageNumber = 1,
            int pageSize = 10,
            Guid? childServiceCategoryId = null,
            string? searchTerm = null);

        Task<IEnumerable<ServiceCategoryDto>> GetParentCategoriesAsync();
        Task<ServiceCategoryDto> CreateCategoryAsync(CreateServiceCategoryDto dto);
        Task<ServiceCategoryDto> UpdateCategoryAsync(Guid id, UpdateServiceCategoryDto dto);
        Task<bool> DeleteCategoryAsync(Guid id);
    }
}
