using BusinessObject.RequestEmergency;
using DataAccessLayer;
using Dtos.Emergency.Dtos.Emergency;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EmergencyRequestRepositories
{
    public class PriceEmergencyRepositories : IPriceEmergencyRepositories
    {
        private readonly MyAppDbContext _context;

        public PriceEmergencyRepositories(MyAppDbContext context)
        {
            _context = context;
        }
        public async Task<PriceEmergency> GetLatestPriceAsync()
        {
            return await _context.PriceEmergencies
                .OrderByDescending(p => p.DateCreated)
                .FirstOrDefaultAsync();
        }

        //  Thêm giá mới
        public async Task AddPriceAsync(PriceEmergencyDto priceDto)
        {
            var price = new PriceEmergency
            {
                BasePrice = priceDto.BasePrice,
                PricePerKm = priceDto.PricePerKm,
                DateCreated = DateTime.Now
            };

            _context.PriceEmergencies.Add(price);
            await _context.SaveChangesAsync();
        }


        // Lấy toàn bộ lịch sử giá
        public async Task<IEnumerable<PriceEmergency>> GetAllPricesAsync()
        {
            return await _context.PriceEmergencies
                .OrderByDescending(p => p.DateCreated)
                .ToListAsync();
        }
    }
}
