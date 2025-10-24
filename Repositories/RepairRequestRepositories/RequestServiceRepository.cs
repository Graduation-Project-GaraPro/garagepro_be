using BusinessObject.Customers;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.RepairRequestRepositories
{
    public class RequestServiceRepository: IRequestServiceRepository
    {
       
            private readonly MyAppDbContext _context;

            public RequestServiceRepository(MyAppDbContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<RequestService>> GetAllAsync()
            {
                return await _context.RequestServices
                    .Include(rs => rs.Service)
                    .Include(rs => rs.RepairRequest)
                    .Include(rs => rs.RequestParts)               // 👈 load Parts
            .ThenInclude(rp => rp.Part)
                    .ToListAsync();
            }

        public async Task<IEnumerable<RequestService>> GetByRepairRequestIdAsync(Guid repairRequestId)
        {
            return await _context.RequestServices
                .Include(rs => rs.Service)
                .Include(rs => rs.RequestParts)
                    .ThenInclude(rp => rp.Part)
                .Where(rs => rs.RepairRequestId == repairRequestId)
                .ToListAsync();
        }

        public async Task<RequestService> GetByIdAsync(Guid id)
        {
            return await _context.RequestServices
                .Include(rs => rs.Service)
                .Include(rs => rs.RepairRequest)
                .Include(rs => rs.RequestParts)
                    .ThenInclude(rp => rp.Part)
                .FirstOrDefaultAsync(rs => rs.RequestServiceId == id);
        }
        public async Task<RequestService> AddAsync(RequestService requestService)
            {
                _context.RequestServices.Add(requestService);
                await _context.SaveChangesAsync();
                return requestService;
            }

            public async Task<RequestService> UpdateAsync(RequestService requestService)
            {
                _context.Entry(requestService).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return requestService;
            }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var deletedCount = await _context.RequestServices
                .Where(rs => rs.RequestServiceId == id)
                .ExecuteDeleteAsync(); // ✅ xóa trực tiếp trong DB, không cần load vào bộ nhớ

            return deletedCount > 0;
        }


        public async Task<bool> ExistsAsync(Guid id)
            {
                return await _context.RequestServices.AnyAsync(rs => rs.RequestServiceId == id);
            }
        }
    }

