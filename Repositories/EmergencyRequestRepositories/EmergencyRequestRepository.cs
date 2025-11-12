using BusinessObject.RequestEmergency;
using DataAccessLayer;
using Dtos.Emergency;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EmergencyRequestRepositories
{
    
        public class EmergencyRequestRepository : IEmergencyRequestRepository
        {
            private readonly MyAppDbContext _context;

            public EmergencyRequestRepository(MyAppDbContext context)
            {
                _context = context;
            }

            public async Task<RequestEmergency> CreateAsync(RequestEmergency request)
            {
                await _context.RequestEmergencies.AddAsync(request);
                await _context.SaveChangesAsync();
                return request;
            }
        public async Task<RequestEmergency> UpdateAsync(RequestEmergency emergency)
        {
            _context.RequestEmergencies.Update(emergency);
            await _context.SaveChangesAsync();
            return emergency;
        }


        public async Task<List<RequestEmergency>> GetAllEmergencyAsync()
        {
            return await _context.RequestEmergencies
                 .Include(r => r.Branch)
                 .Include(r => r.Customer)
                 .Include(r => r.Vehicle)
                 .Include(r => r.MediaFiles)
                 .Include(r=> r.RepairRequest)
                 .ToListAsync();
        }

        public async Task<IEnumerable<RequestEmergency>> GetByCustomerAsync(string customerId)
        {
            return await _context.RequestEmergencies
                .Include(r => r.Branch)
                .Include(r => r.Customer)
                .Include(r=>r.Vehicle)
                .Include(r => r.MediaFiles)
                .Include(r=> r.RepairRequest)
                .Where(r => r.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<RequestEmergency?> GetByIdAsync(Guid id)
        {
            return await _context.RequestEmergencies
                .Include(r => r.Branch)
                .Include(r => r.Customer)
                .Include(r=> r.Vehicle)
                .Include(r => r.MediaFiles)
                .FirstOrDefaultAsync(r => r.EmergencyRequestId == id);
        }


        public async Task<List<BranchNearbyResponseDto>> GetNearestBranchesAsync(double userLat, double userLon, int count = 5)
        {
            var branches = await _context.Branches.ToListAsync();

            var nearestBranches = branches
                .Select(branch => new BranchNearbyResponseDto
                {
                    BranchId = branch.BranchId,
                    BranchName = branch.BranchName,
                    Address = branch.District + branch.Ward,
                    DistanceKm = GetDistance(userLat, userLon, branch.Latitude, branch.Longitude)
                })
                .OrderBy(x => x.DistanceKm)
                .Take(count)
                .ToList();

            return nearestBranches;
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;// bán kính trái đất tính bằng km
            var dLat = ToRadians(lat2 - lat1);// độ vĩ
            var dLon = ToRadians(lon2 - lon1);// độ kinh độ
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);// công thức haversine
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));// góc đo bằng radian
            return R * c;// khoảng cách giữa 2 điểm
        }

        private double ToRadians(double deg) => deg * (Math.PI / 180);
    }
}

