using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Branches;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories.ServiceRepositories
{
    public class ServiceCategoryRepository : IServiceCategoryRepository
    {
        private readonly MyAppDbContext _context;

        public ServiceCategoryRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceCategory>> GetAllAsync()
        {
            return await _context.ServiceCategories
                .Include(sc => sc.Services).ThenInclude(s => s.ServicePartCategories).ThenInclude(s => s.PartCategory).ThenInclude(p => p.Parts)
                .Include(sc => sc.ChildServiceCategories)
                    .ThenInclude(c => c.Services).ThenInclude(s=>s.ServicePartCategories).ThenInclude(s=>s.PartCategory).ThenInclude(p => p.Parts)
                .Include(sc => sc.ChildServiceCategories)
                    .ThenInclude(c => c.ChildServiceCategories) // nếu muốn nhiều cấp
                .ToListAsync();
        }
        public async Task<(IEnumerable<ServiceCategory> Categories, int TotalCount)> GetCategoriesByParentAsync(
                 Guid parentServiceCategoryId,
                 int pageNumber,
                 int pageSize,
                 Guid? childServiceCategoryId = null,
                 string? searchTerm = null,
                 Guid? branchId = null
             )
        {
            var query = _context.ServiceCategories
                // Include services đã filter theo Active + Branch
                .Include(c => c.Services
                    .Where(s =>
                        s.IsActive == true &&
                        (!branchId.HasValue ||
                         s.BranchServices.Any(bs => bs.BranchId == branchId.Value))  
                    )
                )
                    .ThenInclude(s => s.ServicePartCategories)
                        .ThenInclude(spc => spc.PartCategory)
                            .ThenInclude(p => p.Parts)
                .Include(c => c.ChildServiceCategories)
                .Where(c => c.ParentServiceCategoryId == parentServiceCategoryId)
                .AsSplitQuery()
                .AsQueryable();

            // 🔹 Lọc theo childServiceCategoryId
            if (childServiceCategoryId.HasValue)
            {
                query = query.Where(c => c.ServiceCategoryId == childServiceCategoryId.Value);
            }

            // 🔹 Tìm kiếm theo tên category / child / service
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.CategoryName.Contains(searchTerm) ||
                    c.ChildServiceCategories.Any(cc => cc.CategoryName.Contains(searchTerm)) ||
                    c.Services.Any(s =>
                        s.ServiceName.Contains(searchTerm) &&
                        s.IsActive == true &&
                        (!branchId.HasValue ||
                         s.BranchServices.Any(bs => bs.BranchId == branchId.Value)) // ✅ search theo branch
                    )
                );
            }

            // 🔹 Chỉ giữ category có ÍT NHẤT 1 service thoả IsActive + Branch
            query = query.Where(c =>
                c.Services.Any(s =>
                    s.IsActive == true &&
                    (!branchId.HasValue ||
                     s.BranchServices.Any(bs => bs.BranchId == branchId.Value))
                )
            );

            var totalCount = await query.CountAsync();

            var categories = await query
                .OrderBy(c => c.CategoryName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (categories, totalCount);
        }


        public async Task<IEnumerable<ServiceCategory>> GetAllCategoriesWithFilterAsync(
            Guid? parentServiceCategoryId = null,
            string? searchTerm = null,
            bool? isActive = null)
        {
            var query = _context.ServiceCategories
                .Include(c => c.ParentServiceCategory)
                .Include(c => c.ChildServiceCategories)
                .Include(c => c.Services)
                .AsSplitQuery()
                .AsQueryable();

            // 🔹 Nếu có parentServiceCategoryId → chỉ lấy theo parent đó
            if (parentServiceCategoryId.HasValue)
            {
                query = query.Where(c => c.ParentServiceCategoryId == parentServiceCategoryId.Value);
            }

            // 🔹 Lọc theo trạng thái IsActive
            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            // 🔹 Tìm kiếm theo tên (trong category, parent hoặc child)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.CategoryName.Contains(searchTerm) ||
                    (c.ParentServiceCategory != null && c.ParentServiceCategory.CategoryName.Contains(searchTerm)) ||
                    c.ChildServiceCategories.Any(child => child.CategoryName.Contains(searchTerm))
                );
            }

            // 🔹 Trả về toàn bộ danh sách (không phân trang)
            return await query
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }


        public async Task<IEnumerable<ServiceCategory>> GetParentCategoriesAsync()
        {
            return await _context.ServiceCategories           
                .Include(c => c.ChildServiceCategories)      // Load các category con (nếu muốn hiển thị cây)
                .Where(c => c.ParentServiceCategoryId == null)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }
        public async Task<ServiceCategory> GetByIdAsync(Guid id)
        {
            return await _context.ServiceCategories
                .Include(sc => sc.Services)
                .Include(sc => sc.ChildServiceCategories)
                    .ThenInclude(c => c.Services)
                .AsSplitQuery()
                .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == id);
        }

        public async Task<(IEnumerable<ServiceCategory> Categories, int TotalCount)> GetCategoriesForBookingAsync(
        int pageNumber,
        int pageSize,
        Guid? serviceCategoryId = null,
        string? searchTerm = null)
        {
            var query = _context.ServiceCategories
                .Include(c => c.Services)
                    .ThenInclude(s => s.ServicePartCategories).ThenInclude(s => s.PartCategory).ThenInclude(p => p.Parts)
                   .AsSplitQuery()
                .AsQueryable();

            // Filter theo category
            if (serviceCategoryId.HasValue)
            {
                query = query.Where(c => c.ServiceCategoryId == serviceCategoryId);
            }

            // Search theo ServiceName
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Services.Any(s => s.ServiceName.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();

            // Phân trang
            var categories = await query
                .OrderBy(c => c.CategoryName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (categories, totalCount);
        }

        public async Task<IEnumerable<Service>> GetServicesByCategoryIdAsync(Guid categoryId)
        {
            return await _context.Services
                .Where(s => s.ServiceCategoryId == categoryId)
                .ToListAsync();
        }

        public void Add(ServiceCategory category)
        {
            _context.ServiceCategories.Add(category);
        }

        public void Update(ServiceCategory category)
        {
            _context.ServiceCategories.Update(category);
        }

        public void Delete(ServiceCategory category)
        {
            _context.ServiceCategories.Remove(category);
        }
        public IQueryable<ServiceCategory> Query()
        {
            return _context.ServiceCategories
                .Include(sc => sc.Services)
                .Include(sc => sc.ChildServiceCategories)
                    .ThenInclude(c => c.Services)
                .Include(sc => sc.ChildServiceCategories)
                .AsQueryable();
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
