using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Repositories.ServiceRepositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly MyAppDbContext _context;

        public ServiceRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Service>> GetAllAsync()
        {
            return await _context.Services
                .Include(s => s.ServiceCategory).Include(s=>s.BranchServices).ThenInclude(bs=>bs.Branch).Include(s => s.ServicePartCategories).ThenInclude(sp => sp.PartCategory).ThenInclude(p => p.Parts).AsNoTracking()
                .ToListAsync();
        }

        public async Task<Service> GetByIdAsync(Guid id)
        {
            return await _context.Services.AsNoTracking()
                .Include(s => s.ServiceCategory).Include(s => s.BranchServices).ThenInclude(bs => bs.Branch)
                
                .Include(s => s.ServicePartCategories).ThenInclude(sp => sp.PartCategory).ThenInclude(p => p.Parts)
                .FirstOrDefaultAsync(s => s.ServiceId == id);
        }

        public async Task<Service?> GetByIdWithRelationsAsync(Guid id)
        {
            return await _context.Services
                 .Include(s => s.ServiceCategory)
                .Include(s => s.BranchServices).ThenInclude(bs => bs.Branch)
                
                .Include(s => s.ServicePartCategories).ThenInclude(sp => sp.PartCategory).ThenInclude(p => p.Parts)
                .AsSplitQuery()


                .FirstOrDefaultAsync(s => s.ServiceId == id);
        }
        public IQueryable<Service> Query()
        {
            return _context.Services
                .Include(s => s.ServiceCategory)
                .Include(s => s.BranchServices).ThenInclude(bs => bs.Branch)              
                .Include(s => s.ServicePartCategories).ThenInclude(sp => sp.PartCategory).ThenInclude(p=>p.Parts)
                .Include(s => s.RepairOrderServices)
                .Include(s => s.ServiceInspections)
                .Include(s => s.Jobs)
                .Include(s => s.PromotionalCampaignServices)
                .Include(s => s.QuotationServices)
                .Include(s => s.RequestServices)
                .AsSplitQuery()
                .AsQueryable();
            // nếu muốn Include category luôn:
            // return _context.Services.Include(s => s.ServiceCategory).AsQueryable();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
        public async Task AddAsync(Service service)
        {
            await _context.Services.AddAsync(service);
        }

        public void Update(Service service)
        {
            _context.Services.Update(service);
        }

        public void Delete(Service service)
        {
            _context.Services.Remove(service);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

}
