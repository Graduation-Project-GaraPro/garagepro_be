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
    public class RequestPartRepository: IRequestPartRepository
    {
        
            private readonly MyAppDbContext _context;

            public RequestPartRepository(MyAppDbContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<RequestPart>> GetAllAsync()
            {
                return await _context.RequestParts
                    .Include(rp => rp.RepairRequest)
                    .Include(rp => rp.Part)
                    .ToListAsync();
            }

            public async Task<IEnumerable<RequestPart>> GetByRepairRequestIdAsync(Guid repairRequestId)
            {
                return await _context.RequestParts
                    .Include(rp => rp.RepairRequest)
                    .Include(rp => rp.Part)
                    .Where(rp => rp.RepairRequestID == repairRequestId)
                    .ToListAsync();
            }

            public async Task<RequestPart> GetByIdAsync(Guid id)
            {
                return await _context.RequestParts
                    .Include(rp => rp.RepairRequest)
                    .Include(rp => rp.Part)
                    .FirstOrDefaultAsync(rp => rp.RequestPartId == id);
            }

            public async Task<RequestPart> AddAsync(RequestPart requestPart)
            {
                _context.RequestParts.Add(requestPart);
                await _context.SaveChangesAsync();
                return requestPart;
            }

            public async Task<RequestPart> UpdateAsync(RequestPart requestPart)
            {
                _context.Entry(requestPart).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return requestPart;
            }

            public async Task<bool> DeleteAsync(Guid id)
            {
                var requestPart = await _context.RequestParts.FindAsync(id);
                if (requestPart == null)
                    return false;

                _context.RequestParts.Remove(requestPart);
                await _context.SaveChangesAsync();
                return true;
            }

            public async Task<bool> ExistsAsync(Guid id)
            {
                return await _context.RequestParts.AnyAsync(rp => rp.RequestPartId == id);
            }
        }
    }



