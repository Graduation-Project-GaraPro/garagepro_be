using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject;
using BusinessObject.Branches;
using Dtos.Services;

using Microsoft.EntityFrameworkCore;
using Repositories.BranchRepositories;
using Repositories.ServiceRepositories;

namespace Services.ServiceServices
{
    public class ServiceService : IServiceService
    {
    
        private readonly IServiceRepository _repository;
        private readonly IServiceCategoryRepository _serviceCategoryRepository;
        private readonly IBranchRepository _branchRepository;

        private readonly IMapper _mapper;

        public ServiceService(IServiceRepository repository, IMapper mapper, IServiceCategoryRepository serviceCategoryRepository, IBranchRepository branchRepository)
        {
            _repository = repository;
            _serviceCategoryRepository = serviceCategoryRepository;
            _branchRepository = branchRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ServiceDto>> GetAllServicesAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ServiceDto>>(entities);
        }

        public async Task<ServiceDto> GetServiceByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return _mapper.Map<ServiceDto>(entity);
        }

        public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto)
        {
            // Check trùng tên trong cùng category bằng Query()
            var exists = await _repository.Query()
                .AnyAsync(s => s.ServiceName == dto.ServiceName
                            && s.ServiceCategoryId == dto.ServiceCategoryId);

            if (exists)
                throw new ApplicationException($"Service name '{dto.ServiceName}' already exists in this category.");

            var categoryExists = await _serviceCategoryRepository.Query()
              .AnyAsync(c => c.ServiceCategoryId == dto.ServiceCategoryId);

            if (!categoryExists)
                throw new ApplicationException($"ServiceCategoryId '{dto.ServiceCategoryId}' does not exist.");

            // Check tất cả BranchId có tồn tại không
            foreach (var branchId in dto.BranchIds ?? new List<Guid>())
            {
                var branchExists = await _branchRepository.ExistsAsync(b => b.BranchId == branchId);
                if (!branchExists)
                    throw new ApplicationException($"BranchId '{branchId}' does not exist.");
            }


            var entity = _mapper.Map<Service>(dto);
            entity.ServiceId = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            // Thêm BranchService từ BranchIds
            entity.BranchServices = dto.BranchIds.Select(branchId => new BranchService
            {
                BranchId = branchId,
                ServiceId = entity.ServiceId
            }).ToList();


            // Thêm ServicePart từ PartIds
            entity.ServiceParts = dto.PartIds.Select(partId => new ServicePart
            {
                PartId = partId,
                ServiceId = entity.ServiceId
            }).ToList();

            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            return _mapper.Map<ServiceDto>(entity);
        }

        public async Task<ServiceDto> UpdateServiceAsync(Guid id, UpdateServiceDto dto)
        {
            var existing = await _repository.Query()
                .Include(s => s.BranchServices) // load navigation
                .FirstOrDefaultAsync(s => s.ServiceId == id);

            if (existing == null) return null;

            // Check trùng tên (ngoại trừ chính nó)
            var exists = await _repository.Query()
                .AnyAsync(s => s.ServiceName == dto.ServiceName
                            && s.ServiceCategoryId == dto.ServiceCategoryId
                            && s.ServiceId != id);

            if (exists)
                throw new ApplicationException($"Service name '{dto.ServiceName}' already exists in this category.");

            var categoryExists = await _serviceCategoryRepository.Query()
                .AnyAsync(c => c.ServiceCategoryId == dto.ServiceCategoryId);

            if (!categoryExists)
                throw new ApplicationException($"ServiceCategoryId '{dto.ServiceCategoryId}' does not exist.");

            // Check tất cả BranchId có tồn tại không
            foreach (var branchId in dto.BranchIds ?? new List<Guid>())
            {
                var branchExists = await _branchRepository.ExistsAsync(b => b.BranchId == branchId);
                if (!branchExists)
                    throw new ApplicationException($"BranchId '{branchId}' does not exist.");
            }

            // Check tất cả PartId có tồn tại không (nếu bạn có PartRepository)
            //foreach (var partId in dto.PartIds ?? new List<Guid>())
            //{
            //    var partExists = await _partRepository.ExistsAsync(p => p.PartId == partId);
            //    if (!partExists)
            //        throw new ApplicationException($"PartId '{partId}' does not exist.");
            //}
            // Map DTO -> entity (các field cơ bản)
            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            // Đồng bộ BranchServices
            var currentBranchIds = existing.BranchServices.Select(bs => bs.BranchId).ToList();
            var newBranchIds = dto.BranchIds;

            // Xoá những branch không còn trong danh sách mới
            
            foreach (var bs in existing.BranchServices.ToList())
            {
                if (!newBranchIds.Contains(bs.BranchId))
                {
                    existing.BranchServices.Remove(bs);
                }
            }

            // Thêm những branch mới chưa có
            foreach (var branchId in newBranchIds.Except(currentBranchIds))
            {
                existing.BranchServices.Add(new BranchService
                {
                    BranchId = branchId,
                    ServiceId = existing.ServiceId
                });
            }

           
            // Đồng bộ ServiceParts
           
            var currentPartIds = existing.ServiceParts.Select(sp => sp.PartId).ToList();
            var newPartIds = dto.PartIds;

            foreach (var sp in existing.ServiceParts.ToList())
            {
                if (!newPartIds.Contains(sp.PartId))
                {
                    existing.ServiceParts.Remove(sp);
                }
            }

            foreach (var partId in newPartIds.Except(currentPartIds))
            {
                existing.ServiceParts.Add(new ServicePart
                {
                    ServiceId = existing.ServiceId,
                    PartId = partId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _repository.Update(existing);
            await _repository.SaveChangesAsync();

            return _mapper.Map<ServiceDto>(existing);
        }


        public async Task<bool> DeleteServiceAsync(Guid id)
        {
            var existing = await _repository.Query()
                .FirstOrDefaultAsync(s => s.ServiceId == id);

            if (existing == null) return false;

            _repository.Delete(existing);
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
