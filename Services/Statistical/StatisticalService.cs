using AutoMapper;
using DataAccessLayer;
using Dtos.Statistical;
using Microsoft.EntityFrameworkCore;
using Repositories.Statistical;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Statistical
{
    public class StatisticalService : IStatisticalService
    {
        private readonly IStatisticalRepository _repo;
        private readonly IMapper _mapper;
        private readonly MyAppDbContext _context;

        public StatisticalService(IStatisticalRepository repo, IMapper mapper, MyAppDbContext context)
        {
            _repo = repo;
            _mapper = mapper;
            _context = context;
        }

        public async Task<TechnicianStatisticDto> GetTechnicianStatisticAsync(string userId)
        {
            var technician = await _context.Technicians
                .Where(t => t.UserId == userId)
                .Select(t => t.TechnicianId)
                .FirstOrDefaultAsync();

            if (technician == Guid.Empty)
                throw new UnauthorizedAccessException("Không tìm thấy thông tin Technician cho tài khoản này.");

            var techData = await _repo.GetTechnicianWithJobsAsync(technician);
            if (techData == null)
                throw new Exception("Không thể tải dữ liệu thống kê cho Technician này.");

            var jobs = techData.JobTechnicians.Select(jt => jt.Job).ToList();

            return new TechnicianStatisticDto
            {
                Quality = techData.Quality,
                Speed = techData.Speed,
                Efficiency = techData.Efficiency,
                Score = techData.Score,
                NewJobs = jobs.Count(j => j.Status == BusinessObject.Enums.JobStatus.New),
                InProgressJobs = jobs.Count(j => j.Status == BusinessObject.Enums.JobStatus.InProgress),
                CompletedJobs = jobs.Count(j => j.Status == BusinessObject.Enums.JobStatus.Completed),
                OnHoldJobs = jobs.Count(j => j.Status == BusinessObject.Enums.JobStatus.OnHold),
                RecentJobs = techData.JobTechnicians
                    .OrderByDescending(jt => jt.CreatedAt)
                    .Take(3)
                    .Select(jt => new Dtos.Statistical.RecentJobDto
                    {
                        JobName = jt.Job.JobName,
                        LicensePlate = jt.Job.RepairOrder.Vehicle.LicensePlate,
                        Status = jt.Job.Status.ToString(),
                        AssignedAt = jt.CreatedAt
                    })
                    .ToList()
            };
        }
    }
}
