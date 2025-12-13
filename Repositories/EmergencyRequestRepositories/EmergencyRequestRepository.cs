using BusinessObject.InspectionAndRepair;
using BusinessObject.RequestEmergency;
using DataAccessLayer;
using Dtos.Emergency;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.RequestEmergency.RequestEmergency;

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

        public async Task<bool> UpdateEmergencyStatusAsync(
                Guid emergencyRequestId,
                RequestEmergency.EmergencyStatus newStatus,
                string? rejectReason = null,
                string? technicianId = null
            )
        {
            var request = await _context.RequestEmergencies
                .FirstOrDefaultAsync(r => r.EmergencyRequestId == emergencyRequestId);

            if (request == null)
                return false;

            // Cập nhật trạng thái
            request.Status = newStatus;

            // Nếu gara từ chối -> lưu RejectReason + thời gian phản hồi
            if (newStatus == RequestEmergency.EmergencyStatus.Canceled && rejectReason != null)
            {
                request.RejectReason = rejectReason;
            }

            // Nếu gara tiếp nhận -> lưu thời gian phản hồi
            if (newStatus == RequestEmergency.EmergencyStatus.Accepted)
            {
                request.RespondedAt = DateTime.UtcNow;
            }

            // Nếu chỉ định kỹ thuật viên
            if (technicianId != null)
            {
                request.TechnicianId = technicianId;
            }

            // Hoàn thành yêu cầu
            if (newStatus == RequestEmergency.EmergencyStatus.Completed)
            {
                // Bạn có thể thêm logic nếu cần
                // request.CompletedAt = DateTime.UtcNow; (nếu muốn thêm)
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignTechnicianAsync(Guid emergencyId, string technicianId)
        {
            // 1. Find emergency
            var emergency = await _context.RequestEmergencies
                .FirstOrDefaultAsync(e => e.EmergencyRequestId == emergencyId);

            if (emergency == null)
                return false;

            // 2. Check emergency status
            if (emergency.Status != EmergencyStatus.Accepted)
                throw new InvalidOperationException("Only emergencies with 'Accepted' status can be assigned.");

            // 3. Check that technician exists
            var technicianExists = await _context.Users
                .AnyAsync(u => u.Id == technicianId);

            if (!technicianExists)
                throw new InvalidOperationException("Technician does not exist.");

            // 4. Ensure technician is not in another active emergency
            var technicianHasActiveEmergency = await _context.RequestEmergencies
                .AnyAsync(e =>
                    e.TechnicianId == technicianId &&
                    (e.Status == EmergencyStatus.Assigned ||
                     e.Status == EmergencyStatus.InProgress ||
                     e.Status == EmergencyStatus.Towing));

            if (technicianHasActiveEmergency)
                throw new InvalidOperationException("Technician is already handling another emergency.");

            // 5. Assign technician
            emergency.TechnicianId = technicianId;
            emergency.Status = EmergencyStatus.Assigned;

            await _context.SaveChangesAsync();
            return true;
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

        public async Task<bool> AnyActiveAsync(string customerId)
        {
            return await _context.RequestEmergencies.AnyAsync(e =>
                e.CustomerId == customerId &&
                (e.Status == BusinessObject.RequestEmergency.RequestEmergency.EmergencyStatus.Pending
                 || e.Status == BusinessObject.RequestEmergency.RequestEmergency.EmergencyStatus.Accepted
                 || e.Status == BusinessObject.RequestEmergency.RequestEmergency.EmergencyStatus.InProgress
                 || e.Status == BusinessObject.RequestEmergency.RequestEmergency.EmergencyStatus.Towing
                 || e.Status == BusinessObject.RequestEmergency.RequestEmergency.EmergencyStatus.Assigned));
        }

        public async Task<IEnumerable<RequestEmergency>> GetByCustomerAsync(string customerId)
        {
            return await _context.RequestEmergencies
                .Include(r => r.Branch)
                .Include(r => r.Customer)
                .Include(r=>r.Vehicle)
                .Include(r=>r.Technician)
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
                .Include(r=>r.Technician)
                .Include(r => r.MediaFiles)
                .FirstOrDefaultAsync(r => r.EmergencyRequestId == id);
        }


        public async Task<RequestEmergency?> GetCurrentEmergencyForTechnicianAsync(string technicianId)
        {
            return await _context.Set<RequestEmergency>()
                .Include(x => x.Customer)
                .Include(x => x.Branch)
                .Include(x => x.Vehicle)
                .Include(x => x.MediaFiles)
                .Where(x =>
                    x.TechnicianId == technicianId &&
                    (x.Status == RequestEmergency.EmergencyStatus.Assigned ||
                     x.Status == RequestEmergency.EmergencyStatus.InProgress
                     || x.Status == RequestEmergency.EmergencyStatus.Towing
                     )
                     
                     )
                .OrderByDescending(x => x.RequestTime)
                .FirstOrDefaultAsync();
        }

       
        public async Task<List<RequestEmergency>> GetEmergencyListForTechnicianAsync(string technicianId)
        {
            return await _context.Set<RequestEmergency>()
                .Include(x => x.Customer)
                .Include(x => x.Branch)
                .Include(x => x.Vehicle)
                .Where(x => x.TechnicianId == technicianId)
                .OrderByDescending(x => x.RequestTime)
                .ToListAsync();
        }


        public async Task<List<BranchNearbyResponseDto>> GetNearestBranchesAsync(double userLat, double userLon, int count = 5)
        {
            var branches = await _context.Branches
                .Where(b => b.IsActive)
                .ToListAsync();

            var nearestBranches = branches
                .Select(branch => new BranchNearbyResponseDto
                {
                    BranchId = branch.BranchId,
                    BranchName = branch.BranchName,
                    PhoneNumber = branch.PhoneNumber,
                    Address = string.Join(", ", new[] { branch.Street, branch.Commune, branch.Province }.Where(s => !string.IsNullOrWhiteSpace(s))),
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

        public async Task<bool> AssignTechnicianAsync(Guid emergencyId, Guid technicianUserId)
        {
            var request = _context.RequestEmergencies.FirstOrDefault(e => e.EmergencyRequestId.Equals( emergencyId));
            if (request == null)
            {
                return false;
            }
            if (request.Status == BusinessObject.RequestEmergency.RequestEmergency.EmergencyStatus.Pending)
            {
                throw new InvalidOperationException("Cannot assign technician to a pending emergency request.");
            }
            // 2. Check emergency status
            if (request.Status != EmergencyStatus.Accepted)
                throw new InvalidOperationException("Only emergencies with 'Accepted' status can be assigned.");

            // 3. Check that technician exists
            var technicianExists = await _context.Users
                .AnyAsync(u => u.Id == technicianUserId.ToString());

            if (!technicianExists)
                throw new InvalidOperationException("Technician does not exist.");

            // 4. Ensure technician is not in another active emergency
            var technicianHasActiveEmergency = await _context.RequestEmergencies
                .AnyAsync(e =>
                    e.TechnicianId == technicianUserId.ToString() &&
                    (e.Status == EmergencyStatus.Assigned ||
                     e.Status == EmergencyStatus.InProgress ||
                     e.Status == EmergencyStatus.Towing));

            if (technicianHasActiveEmergency)
                throw new InvalidOperationException("Technician is already handling another emergency.");
            request.TechnicianId = technicianUserId.ToString();
            request.Status = BusinessObject.RequestEmergency.RequestEmergency.EmergencyStatus.Assigned;
            _context.RequestEmergencies.Update(request);
            return await _context.SaveChangesAsync().ContinueWith(t => t.Result > 0);
        }
    }
}

