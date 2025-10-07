using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dtos.Branches;
using Dtos.Services;
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

        public async Task<ServiceCategoryDto> GetCategoryByIdAsync(Guid id)
        {
            var category = await _repository.GetByIdAsync(id);
            return _mapper.Map<ServiceCategoryDto>(category);
        }

        public async Task<IEnumerable<ServiceDto>> GetServicesByCategoryIdAsync(Guid categoryId)
        {
            var services = await _repository.GetServicesByCategoryIdAsync(categoryId);
            return _mapper.Map<IEnumerable<ServiceDto>>(services);
        }
    }
}
