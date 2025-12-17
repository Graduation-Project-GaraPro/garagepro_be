using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject;
using BusinessObject.Branches;
using DataAccessLayer;
using Dtos.Services;

using Microsoft.EntityFrameworkCore;
using Repositories.BranchRepositories;
using Repositories.PartRepositories;
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
        private readonly IPartCategoryRepository _partCategoryRepository;
        private readonly MyAppDbContext _context;
        private readonly IMapper _mapper;

        public ServiceService(IServiceRepository repository, IMapper mapper, IServiceCategoryRepository serviceCategoryRepository, IBranchRepository branchRepository, IPartRepository partRepository, MyAppDbContext context, IPartCategoryRepository partCategoryRepository)
        {
            _repository = repository;
            _serviceCategoryRepository = serviceCategoryRepository;
            _branchRepository = branchRepository;
            _partRepository = partRepository;
            _partCategoryRepository = partCategoryRepository;
            _context = context;
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
                .OrderBy(s=>s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsSplitQuery()
                .ToListAsync();

            var services = _mapper.Map<IEnumerable<ServiceDto>>(pagedEntities);

            return (services, totalCount);
        }


        public async Task<ServiceDto> GetServiceByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity?.ServicePartCategories?.Any() == true)
            {
               
                        entity.ServicePartCategories = entity.ServicePartCategories
                            .GroupBy(
                                p => p.PartCategory.CategoryName?.Trim() ?? string.Empty,
                                StringComparer.OrdinalIgnoreCase)
                            .Select(g => g.First())
                            .ToList();
                    
                
            }

            return _mapper.Map<ServiceDto>(entity);
        }

        public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto)
        {
            using var transaction = await _repository.BeginTransactionAsync();

            try
            {
                // 1) Kiểm tra trùng tên trong cùng Category
                var exists = await _repository.Query()
                    .AnyAsync(s => s.ServiceName.ToLower() == dto.ServiceName.ToLower()
                                && s.ServiceCategoryId == dto.ServiceCategoryId);

                if (exists)
                    throw new ApplicationException($"Service name '{dto.ServiceName}' already exists in this category.");

                // 2) Kiểm tra ServiceCategory tồn tại
                var category = await _serviceCategoryRepository.Query()
                    .FirstOrDefaultAsync(c => c.ServiceCategoryId == dto.ServiceCategoryId);

                if (category == null)
                    throw new ApplicationException($"ServiceCategoryId '{dto.ServiceCategoryId}' does not exist.");

                // 3) Không cho thêm Service vào Category cha
                if (category.ParentServiceCategoryId == null)
                    throw new ApplicationException("Cannot add a service directly to a top-level (parent) category. Please select a subcategory.");

                // 4) Kiểm tra Branch hợp lệ
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

                // 5) Validate PartCategory theo TÊN + lấy ID tương ứng
                var normalizedNames = (dto.PartCategoryNames ?? new List<string>())
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(n => n.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (normalizedNames.Count > 1 && dto.IsAdvanced != true)
                    throw new ApplicationException("Basic Service should contain exactly 1 Part Category.");

                
                var partCategoryIds = new List<Guid>();
                if (normalizedNames.Any())
                {
                    // Giả sử entity PartCategory có field Name là PartCategoryName (bạn đổi theo đúng tên cột)
                    var matchedParts = await _partCategoryRepository.Query()
                        .Where(p => normalizedNames.Contains(p.CategoryName))  
                        .Select(p => new { p.LaborCategoryId, p.CategoryName }) 
                        .ToListAsync();

                    var matchedNamesSet = matchedParts
                        .Select(x => x.CategoryName)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var invalidNames = normalizedNames
                        .Where(n => !matchedNamesSet.Contains(n))
                        .ToList();

                    if (invalidNames.Any())
                        throw new ApplicationException($"The following PartCategory names do not exist: {string.Join(", ", invalidNames)}");

                    partCategoryIds = matchedParts.Select(x => x.LaborCategoryId).Distinct().ToList();
                }

                // 6) Map sang entity và thêm quan hệ
                var entity = _mapper.Map<Service>(dto);
                entity.ServiceId = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;

                entity.BranchServices = dto.BranchIds.Select(branchId => new BranchService
                {
                    BranchId = branchId,
                    ServiceId = entity.ServiceId
                }).ToList();

                entity.ServicePartCategories = partCategoryIds.Select(partCategoryId => new ServicePartCategory
                {
                    PartCategoryId = partCategoryId,
                    ServiceId = entity.ServiceId
                }).ToList();

                await _repository.AddAsync(entity);
                await _repository.SaveChangesAsync();

                await transaction.CommitAsync();
                return _mapper.Map<ServiceDto>(entity);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<ServiceDto> UpdateServiceAsync(Guid id, UpdateServiceDto dto)
        {
            using var transaction = await _repository.BeginTransactionAsync();

            try
            {
                // 0) Validate route id vs dto id
                if (dto.ServiceId != Guid.Empty && dto.ServiceId != id)
                    throw new ApplicationException($"ServiceId '{dto.ServiceId}' does not the same with query request id.");

                dto.ServiceId = id;

                // 1) Load service + relations
                var existing = await _repository.Query()
                    .Include(s => s.BranchServices)
                    .Include(s => s.ServicePartCategories)
                    .FirstOrDefaultAsync(s => s.ServiceId == id);

                if (existing == null)
                    throw new ApplicationException($"ServiceId '{id}' does not exist.");

                // 2) Check duplicate name in same category (exclude itself)
                var exists = await _repository.Query()
                    .AnyAsync(s => s.ServiceId != id
                                && s.ServiceName.ToLower() == dto.ServiceName.ToLower()
                                && s.ServiceCategoryId == dto.ServiceCategoryId);

                if (exists)
                    throw new ApplicationException($"Service name '{dto.ServiceName}' already exists in this category.");

                // 3) Validate category exists + not top-level parent
                var category = await _serviceCategoryRepository.Query()
                    .FirstOrDefaultAsync(c => c.ServiceCategoryId == dto.ServiceCategoryId);

                if (category == null)
                    throw new ApplicationException($"ServiceCategoryId '{dto.ServiceCategoryId}' does not exist.");

                if (category.ParentServiceCategoryId == null)
                    throw new ApplicationException("Cannot assign a service directly to a top-level (parent) category. Please select a subcategory.");

                // 4) Validate branches
                var branchIds = dto.BranchIds?.Distinct().ToList() ?? new List<Guid>();
                if (branchIds.Count == 0)
                    throw new ApplicationException("At least one branch must be assigned.");

                var validBranchIds = await _branchRepository.Query()
                    .Where(b => branchIds.Contains(b.BranchId))
                    .Select(b => b.BranchId)
                    .ToListAsync();

                var invalidBranchIds = branchIds.Except(validBranchIds).ToList();
                if (invalidBranchIds.Any())
                    throw new ApplicationException($"The following BranchIds do not exist: {string.Join(", ", invalidBranchIds)}");

                // 5) PartCategoryNames -> PartCategoryIds
                var normalizedNames = (dto.PartCategoryNames ?? new List<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (normalizedNames.Count > 1 && dto.IsAdvanced != true)
                    throw new ApplicationException("Basic Service should contain exactly 1 Part Category.");

                var partCategoryIds = new List<Guid>();
                if (normalizedNames.Any())
                {
                    // NOTE: đổi CategoryName/LaborCategoryId theo đúng field của PartCategory bạn
                    var matched = await _partCategoryRepository.Query()
                        .Where(p => normalizedNames.Contains(p.CategoryName))
                        .Select(p => new { p.LaborCategoryId, p.CategoryName })
                        .ToListAsync();

                    var matchedNames = matched
                        .Select(x => x.CategoryName)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var invalidNames = normalizedNames
                        .Except(matchedNames, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (invalidNames.Any())
                        throw new ApplicationException($"The following PartCategory names do not exist: {string.Join(", ", invalidNames)}");

                    partCategoryIds = matched
                        .Select(x => x.LaborCategoryId)
                        .Distinct()
                        .ToList();
                }

                // 6) Update base fields
                existing.ServiceCategoryId = dto.ServiceCategoryId;
                existing.ServiceName = dto.ServiceName.Trim();
                existing.Description = dto.Description;
                existing.Price = dto.Price;
                existing.EstimatedDuration = dto.EstimatedDuration;
                existing.IsActive = dto.IsActive;
                existing.IsAdvanced = dto.IsAdvanced;
                existing.UpdatedAt = DateTime.UtcNow;

                // 7) Sync BranchServices (remove missing + add new)
                var currentBranchIds = existing.BranchServices?.Select(bs => bs.BranchId).ToList() ?? new List<Guid>();

                foreach (var bs in existing.BranchServices.Where(bs => !branchIds.Contains(bs.BranchId)).ToList())
                    existing.BranchServices.Remove(bs);

                foreach (var addId in branchIds.Except(currentBranchIds))
                {
                    existing.BranchServices.Add(new BranchService
                    {
                        BranchId = addId,
                        ServiceId = existing.ServiceId
                    });
                }

                // 8) Sync ServicePartCategories (an toàn với PK riêng ServicePartCategoryId)
                var existingMappings = await _context.ServicePartCategories
                    .Where(spc => spc.ServiceId == existing.ServiceId)
                    .ToListAsync();

                var currentPartIds = existingMappings.Select(spc => spc.PartCategoryId).ToList();

                var toDelete = existingMappings
                    .Where(spc => !partCategoryIds.Contains(spc.PartCategoryId))
                    .ToList();

                if (toDelete.Any())
                    _context.ServicePartCategories.RemoveRange(toDelete);

                var toAddIds = partCategoryIds.Except(currentPartIds).ToList();
                if (toAddIds.Any())
                {
                    var toAdd = toAddIds.Select(partCateId => new ServicePartCategory
                    {
                        ServicePartCategoryId = Guid.NewGuid(), 
                        ServiceId = existing.ServiceId,
                        PartCategoryId = partCateId,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    _context.ServicePartCategories.AddRange(toAdd);
                }

                // 9) Save
                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();

                // Load lại để DTO phản ánh đúng mapping mới
                var updated = await _repository.GetByIdWithRelationsAsync(id);
                return _mapper.Map<ServiceDto>(updated);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                throw new ApplicationException(
                    "Service has been modified or deleted by another process. Please reload data and try again.",
                    ex
                );
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
