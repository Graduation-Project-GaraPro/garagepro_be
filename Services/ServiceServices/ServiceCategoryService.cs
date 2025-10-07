using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject;
using Dtos.Branches;
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
