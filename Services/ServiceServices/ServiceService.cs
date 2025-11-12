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
using Repositories.PartRepositories;
using Repositories.ServiceRepositories;

namespace Services.ServiceServices
{
    public class ServiceService : IServiceService
    {
    
        private readonly IServiceRepository _repository;
        private readonly IServiceCategoryRepository _serviceCategoryRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IPartRepository _partRepository;

        private readonly IMapper _mapper;

        public ServiceService(IServiceRepository repository, IMapper mapper, IServiceCategoryRepository serviceCategoryRepository, IBranchRepository branchRepository, IPartRepository partRepository)
        {
            _repository = repository;
            _serviceCategoryRepository = serviceCategoryRepository;
            _branchRepository = branchRepository;
            _partRepository = partRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ServiceDto>> GetAllServicesAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ServiceDto>>(entities);
        }


        public async Task<(IEnumerable<ServiceDto> Services, int TotalCount)> GetPagedServicesAsync(
     int pageNumber, int pageSize, string? searchTerm, bool? status, Guid? serviceTypeId)
        {
            var query = _repository.Query(); // IQueryable<Service>

            // 🔸 Nếu có filter theo ServiceCategory
            if (serviceTypeId.HasValue)
            {
                // Lấy danh mục được chọn
                var category = await _serviceCategoryRepository.Query()
                    .FirstOrDefaultAsync(c => c.ServiceCategoryId == serviceTypeId.Value);

                if (category != null)
                {
                    // Nếu là cha => lấy tất cả ID của các con
                    if (category.ChildServiceCategories != null && category.ChildServiceCategories.Any())
                    {
                        var childIds = category.ChildServiceCategories.Select(c => c.ServiceCategoryId).ToList();

                        // Lấy service của tất cả danh mục con
                        query = query.Where(s => childIds.Contains(s.ServiceCategoryId));
                    }
                    else
                    {
                        // Nếu là con => chỉ lấy service của chính nó
                        query = query.Where(s => s.ServiceCategoryId == serviceTypeId.Value);
                    }
                }
            }

            //  Search theo tên hoặc mô tả
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s =>
                    s.ServiceName.Contains(searchTerm) ||
                    (s.Description != null && s.Description.Contains(searchTerm)));
            }

            //  Filter theo trạng thái
            if (status.HasValue)
            {
                query = query.Where(s => s.IsActive == status.Value);
            }

            //  Đếm tổng số bản ghi
            var totalCount = await query.CountAsync();

            //  Lấy dữ liệu phân trang
            var pagedEntities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var services = _mapper.Map<IEnumerable<ServiceDto>>(pagedEntities);

            return (services, totalCount);
        }


        public async Task<ServiceDto> GetServiceByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return _mapper.Map<ServiceDto>(entity);
        }

        public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto)
        {
            using var transaction = await _repository.BeginTransactionAsync(); // 👈 mở transaction

            try
            {
                // 1️⃣ Kiểm tra trùng tên trong cùng Category
                var exists = await _repository.Query()
                    .AnyAsync(s => s.ServiceName.ToLower() == dto.ServiceName.ToLower()
                                && s.ServiceCategoryId == dto.ServiceCategoryId);

                if (exists)
                    throw new ApplicationException($"Service name '{dto.ServiceName}' already exists in this category.");

                // 2️⃣ Kiểm tra ServiceCategory tồn tại
                var category = await _serviceCategoryRepository.Query()
                    .FirstOrDefaultAsync(c => c.ServiceCategoryId == dto.ServiceCategoryId);

                if (category == null)
                    throw new ApplicationException($"ServiceCategoryId '{dto.ServiceCategoryId}' does not exist.");

                // 3️⃣ Không cho thêm Service vào Category cha
                if (category.ParentServiceCategoryId == null)
                    throw new ApplicationException("Cannot add a service directly to a top-level (parent) category. Please select a subcategory.");

                // 4️⃣ Kiểm tra Branch & Part hợp lệ
                if (dto.BranchIds?.Any() == true)
                {
                    var validBranchIds = await _branchRepository.Query()
                        .Where(b => dto.BranchIds.Contains(b.BranchId))
                        .Select(b => b.BranchId)
                        .ToListAsync();

                    var invalidBranchIds = dto.BranchIds.Except(validBranchIds).ToList();
                    if (invalidBranchIds.Any())
                        throw new ApplicationException($"The following BranchIds do not exist: {string.Join(", ", invalidBranchIds)}");
                }

                if (dto.PartIds?.Any() == true)
                {
                    var validPartIds = await _partRepository.Query()
                        .Where(p => dto.PartIds.Contains(p.PartId))
                        .Select(p => p.PartId)
                        .ToListAsync();

                    var invalidPartIds = dto.PartIds.Except(validPartIds).ToList();
                    if (invalidPartIds.Any())
                        throw new ApplicationException($"The following PartIds do not exist: {string.Join(", ", invalidPartIds)}");
                }

                // 5️⃣ Map sang entity và thêm quan hệ
                var entity = _mapper.Map<Service>(dto);
                entity.ServiceId = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;

                entity.BranchServices = dto.BranchIds.Select(branchId => new BranchService
                {
                    BranchId = branchId,
                    ServiceId = entity.ServiceId
                }).ToList();

                entity.ServiceParts = dto.PartIds.Select(partId => new ServicePart
                {
                    PartId = partId,
                    ServiceId = entity.ServiceId
                }).ToList();

                await _repository.AddAsync(entity);
                await _repository.SaveChangesAsync();

                await transaction.CommitAsync(); // ✅ Commit khi tất cả thành công
                return _mapper.Map<ServiceDto>(entity);
            }
            catch
            {
                await transaction.RollbackAsync(); // ❌ Rollback nếu có lỗi
                throw;
            }
        }


        public async Task<ServiceDto> UpdateServiceAsync(Guid id, UpdateServiceDto dto)
        {
            using var transaction = await _repository.BeginTransactionAsync();

            try
            {
                var existing = await _repository.GetByIdWithRelationsAsync(id);
                if (existing == null)
                    throw new ApplicationException("Service not found.");

                // Check trùng tên
                var exists = await _repository.Query()
                    .AnyAsync(s => s.ServiceName.ToLower() == dto.ServiceName.ToLower()
                                && s.ServiceCategoryId == dto.ServiceCategoryId
                                && s.ServiceId != id);

                if (exists)
                    throw new ApplicationException($"Service name '{dto.ServiceName}' already exists in this category.");

                //  Check Category hợp lệ
                var category = await _serviceCategoryRepository.Query()
                    .FirstOrDefaultAsync(c => c.ServiceCategoryId == dto.ServiceCategoryId);

                if (category == null)
                    throw new ApplicationException($"ServiceCategoryId '{dto.ServiceCategoryId}' does not exist.");

                if (category.ParentServiceCategoryId == null)
                    throw new ApplicationException("Cannot assign a service to a top-level (parent) category. Please select a subcategory.");

                //  Check Branch & Part hợp lệ
                if (dto.BranchIds?.Any() == true)
                {
                    var validBranchIds = await _branchRepository.Query()
                        .Where(b => dto.BranchIds.Contains(b.BranchId))
                        .Select(b => b.BranchId)
                        .ToListAsync();

                    var invalidBranchIds = dto.BranchIds.Except(validBranchIds).ToList();
                    if (invalidBranchIds.Any())
                        throw new ApplicationException($"The following BranchIds do not exist: {string.Join(", ", invalidBranchIds)}");
                }

                if (dto.PartIds?.Any() == true)
                {
                    var validPartIds = await _partRepository.Query()
                        .Where(p => dto.PartIds.Contains(p.PartId))
                        .Select(p => p.PartId)
                        .ToListAsync();

                    var invalidPartIds = dto.PartIds.Except(validPartIds).ToList();
                    if (invalidPartIds.Any())
                        throw new ApplicationException($"The following PartIds do not exist: {string.Join(", ", invalidPartIds)}");
                }

                //  Update thông tin
                existing.ServiceName = dto.ServiceName;
                existing.Description = dto.Description;
                existing.Price = dto.Price;
                existing.EstimatedDuration = dto.EstimatedDuration;
                existing.ServiceCategoryId = dto.ServiceCategoryId;
                existing.IsActive = dto.IsActive;
                existing.IsAdvanced = dto.IsAdvanced;
                existing.UpdatedAt = DateTime.UtcNow;

                //  Đồng bộ BranchServices
                var currentBranchIds = existing.BranchServices.Select(bs => bs.BranchId).ToList();
                foreach (var bs in existing.BranchServices.Where(bs => !dto.BranchIds.Contains(bs.BranchId)).ToList())
                    existing.BranchServices.Remove(bs);

                foreach (var branchId in dto.BranchIds.Except(currentBranchIds))
                    existing.BranchServices.Add(new BranchService { BranchId = branchId, ServiceId = existing.ServiceId });

                // 🔁 Đồng bộ ServiceParts
                var currentPartIds = existing.ServiceParts.Select(sp => sp.PartId).ToList();
                foreach (var sp in existing.ServiceParts.Where(sp => !dto.PartIds.Contains(sp.PartId)).ToList())
                    existing.ServiceParts.Remove(sp);

                foreach (var partId in dto.PartIds.Except(currentPartIds))
                    existing.ServiceParts.Add(new ServicePart
                    {
                        ServiceId = existing.ServiceId,
                        PartId = partId,
                        CreatedAt = DateTime.UtcNow
                    });

                _repository.Update(existing);
                await _repository.SaveChangesAsync();

                await transaction.CommitAsync();
                return _mapper.Map<ServiceDto>(existing);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<IEnumerable<ServiceDto>> BulkUpdateServiceStatusAsync(List<Guid> serviceIds, bool isActive)
        {
            var services = await _repository.Query()
                .Where(s => serviceIds.Contains(s.ServiceId))
                .ToListAsync();

            if (!services.Any())
                throw new ApplicationException("No matching services found.");

            foreach (var service in services)
            {
                service.IsActive = isActive;
                service.UpdatedAt = DateTime.UtcNow;
            }

            await _repository.SaveChangesAsync();

            return _mapper.Map<IEnumerable<ServiceDto>>(services);
        }

        public async Task<IEnumerable<ServiceDto>> BulkUpdateServiceAdvanceStatusAsync(List<Guid> serviceIds, bool isAdvanced)
        {
            var services = await _repository.Query()
                .Where(s => serviceIds.Contains(s.ServiceId))
                .ToListAsync();

            if (!services.Any())
                throw new ApplicationException("No matching services found.");



            foreach (var service in services)
            {
                service.IsAdvanced = isAdvanced;
                service.UpdatedAt = DateTime.UtcNow;
            }

            await _repository.SaveChangesAsync();

            return _mapper.Map<IEnumerable<ServiceDto>>(services);
        }


        public async Task<bool> DeleteServiceAsync(Guid id)
        {
            var existing = await _repository.Query()
                .Include(s => s.RepairOrderServices)
                .Include(s => s.ServiceInspections)
                .Include(s => s.Jobs)
                .Include(s => s.PromotionalCampaignServices)
                .Include(s => s.QuotationServices)
                .Include(s => s.RequestServices)
                .FirstOrDefaultAsync(s => s.ServiceId == id);

            if (existing == null)
                return false;

            // Kiểm tra quan hệ — nếu tồn tại bất kỳ liên kết nào thì không được xóa
            bool hasRelations =
                (existing.RepairOrderServices?.Any() ?? false) ||
                (existing.ServiceInspections?.Any() ?? false) ||
                (existing.Jobs?.Any() ?? false) ||
                (existing.PromotionalCampaignServices?.Any() ?? false) ||
                (existing.QuotationServices?.Any() ?? false) ||
                (existing.RequestServices?.Any() ?? false);

            if (hasRelations)
                throw new InvalidOperationException("Cannot delete this service because it is currently in use in related records.");

            _repository.Delete(existing);
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
