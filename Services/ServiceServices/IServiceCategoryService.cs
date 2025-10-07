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

        Task<ServiceCategoryDto> CreateCategoryAsync(CreateServiceCategoryDto dto);
        Task<ServiceCategoryDto> UpdateCategoryAsync(Guid id, UpdateServiceCategoryDto dto);
        Task<bool> DeleteCategoryAsync(Guid id);
    }
}
