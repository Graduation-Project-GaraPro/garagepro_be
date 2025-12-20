using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using Microsoft.EntityFrameworkCore;

namespace Repositories.ServiceRepositories
{
    public interface IServiceCategoryRepository
    {
        Task<IEnumerable<ServiceCategory>> GetAllAsync();
        Task<ServiceCategory> GetByIdAsync(Guid id);
        Task<IEnumerable<Service>> GetServicesByCategoryIdAsync(Guid categoryId);

        Task<(IEnumerable<ServiceCategory> Categories, int TotalCount)> GetCategoriesByParentAsync(
            Guid parentServiceCategoryId,
            int pageNumber,
            int pageSize,
            Guid? vehicleId = null,
            Guid? childServiceCategoryId = null,
            string? searchTerm = null,
            Guid? branchId = null
            );

        Task<IEnumerable<ServiceCategory>> GetAllCategoriesWithFilterAsync(
            Guid? parentServiceCategoryId = null,
            string? searchTerm = null,
            bool? isActive = null);
        Task<(IEnumerable<ServiceCategory> Categories, int TotalCount)> GetCategoriesForBookingAsync(
        int pageNumber,
        int pageSize,
        Guid? serviceCategoryId = null,
        string? searchTerm = null);
        Task<IEnumerable<ServiceCategory>> GetParentCategoriesAsync();
        
        IQueryable<ServiceCategory> Query();
        void Add(ServiceCategory category);
        void Update(ServiceCategory category);
        void Delete(ServiceCategory category);
        Task<int> SaveChangesAsync();
    }
}
