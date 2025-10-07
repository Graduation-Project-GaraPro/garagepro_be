using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

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
                .ToListAsync();
        }
    }

}
