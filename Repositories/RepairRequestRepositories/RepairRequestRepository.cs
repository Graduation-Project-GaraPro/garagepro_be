using BusinessObject.Customers;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Customers
{
    public class RepairRequestRepository : IRepairRequestRepository
    {
        private readonly MyAppDbContext _context;

        public RepairRequestRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RepairRequest>> GetAllAsync()
        {
            return await _context.RepairRequests
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.Brand)
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.Model)
                .Include(r => r.RequestServices)
                    .ThenInclude(rs => rs.Service)
                  .Include(r => r.RequestServices)
        .ThenInclude(rs => rs.RequestParts)
            .ThenInclude(rp => rp.Part)
                .Include(r => r.RepairImages)
                .Include(r => r.Customer)
                .Include(r=>r.RequestEmergency)
                .ToListAsync();
        }

        public async Task<IEnumerable<RepairRequest>> GetByUserIdAsync(string userId)
        {
            return await _context.RepairRequests
                .Include(r => r.Vehicle)
                .Include(r => r.RequestServices)
                    .ThenInclude(rs => rs.Service)
                 .Include(r => r.RequestServices)
        .ThenInclude(rs => rs.RequestParts)
            .ThenInclude(rp => rp.Part)
                .Include(r => r.RepairImages)
                .Include(r=> r.RequestEmergency)      
                .Where(r => r.UserID == userId)
                .ToListAsync();
        }
        //chỉ get về để update
        public async Task<RepairRequest> GetTrackingByIdAsync(Guid id)
        {
            return await _context.RepairRequests
                .Include(r => r.RepairImages)
                .Include(r => r.RequestServices)
                    .ThenInclude(rs => rs.RequestParts)
                .FirstOrDefaultAsync(r => r.RepairRequestID == id);
        }
        public async Task<RepairRequest> GetByIdAsync(Guid id)
        {
            return await _context.RepairRequests
                .Include(r => r.Vehicle)
                .Include(r=>r.Branch)
                .Include(r => r.RepairImages)
                .Include(r => r.RequestServices)
                    .ThenInclude(rs => rs.Service)
                .Include(r => r.RequestServices)
                    .ThenInclude(rs => rs.RequestParts)
                        .ThenInclude(rp => rp.Part)
                                .FirstOrDefaultAsync(r => r.RepairRequestID == id);
        }

        // New method for managers with full details
        public async Task<RepairRequest> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.RepairRequests
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.Brand)
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.Model)
                .Include(r => r.RequestServices)
                    .ThenInclude(rs => rs.Service)
                .Include(r => r.RepairImages)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.RepairRequestID == id);
        }

        public async Task<RepairRequest> AddAsync(RepairRequest repairRequest)
        {
            _context.RepairRequests.Add(repairRequest);
            await _context.SaveChangesAsync();
            return repairRequest;
        }

        public async Task<bool> UpdateAsync(RepairRequest repairRequest)
        {
            var entry = _context.Entry(repairRequest);
            if (entry.State == EntityState.Detached)
            {
                _context.Attach(repairRequest);
                entry.State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> DeleteAsync(Guid id)
        {
            var repairRequest = await _context.RepairRequests.FindAsync(id);
            if (repairRequest == null)
                return false;

            _context.RepairRequests.Remove(repairRequest);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.RepairRequests.AnyAsync(r => r.RepairRequestID == id);
        }

        // RepairImages
        public async Task<IEnumerable<RepairImage>> GetImagesAsync(Guid requestId)
        {
            return await _context.RepairImages
                .Where(i => i.RepairRequestId == requestId)
                .ToListAsync();
        }

        public async Task<RepairImage> AddImageAsync(RepairImage image)
        {
            _context.RepairImages.Add(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<bool> DeleteImageAsync(Guid imageId)
        {
            var image = await _context.RepairImages.FindAsync(imageId);
            if (image == null)
                return false;

            _context.RepairImages.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }

        public IQueryable<RepairRequest> GetQueryable()
        {
            return _context.RepairRequests.AsQueryable();
        }
        //get về giá trị một repairRequest theo emergencyId
        public async Task<RepairRequest?> GetByEmergencyIdAsync(Guid emergencyId)
        {
            return await _context.RepairRequests
                .Include(r => r.Vehicle)
                .Include(r => r.Customer)
                .Include(r => r.Branch)
                .FirstOrDefaultAsync(r => r.EmergencyRequestId == emergencyId);
        }

    }
}