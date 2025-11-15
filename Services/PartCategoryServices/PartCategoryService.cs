using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dtos.Parts;
using Repositories.PartCategoryRepositories;

namespace Services.PartCategoryServices
{
    public class PartCategoryService : IPartCategoryService
    {
        private readonly IPartCategoryRepository _repository;
        private readonly IMapper _mapper;

        public PartCategoryService(IPartCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PartCategoryWithPartsDto>> GetAllWithPartsAsync()
        {
            var categories = await _repository.GetAllWithPartsAsync();
            return _mapper.Map<IEnumerable<PartCategoryWithPartsDto>>(categories);
        }

        public async Task<PartCategoryWithPartsDto?> GetByIdWithPartsAsync(Guid id)
        {
            var category = await _repository.GetByIdWithPartsAsync(id);
            return _mapper.Map<PartCategoryWithPartsDto?>(category);
        }

        public async Task<IEnumerable<PartCategoryWithPartsDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? categoryName)
        {
            var categories = await _repository.GetPagedAsync(pageNumber, pageSize, categoryName);
            return _mapper.Map<IEnumerable<PartCategoryWithPartsDto>>(categories);
        }
    }
}
