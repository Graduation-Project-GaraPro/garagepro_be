using AutoMapper;
using BusinessObject.Enums;
using DataAccessLayer;
using Dtos.RepairHistory;
using Microsoft.EntityFrameworkCore;
using Repositories.RepairHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.RepairHistory
{
    public class RepairHistoryService : IRepairHistoryService
    {
        private readonly IRepairHistoryRepository _repo;
        private readonly IMapper _mapper;
        private readonly MyAppDbContext _context;

        public RepairHistoryService(IRepairHistoryRepository repo, IMapper mapper, MyAppDbContext context)
        {
            _repo = repo;
            _mapper = mapper;
            _context = context;
        }

        public async Task<List<RepairHistoryDto>> GetRepairHistoryByUserIdAsync(string userId)
        {
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (technician == null)
                throw new UnauthorizedAccessException("Không tìm thấy thông tin Technician.");

            var technicianData = await _repo.GetTechnicianWithCompletedJobsAsync(technician.TechnicianId);
            if (technicianData == null)
                throw new Exception("Không thể tải dữ liệu lịch sử sửa chữa.");

            var completedJobs = technicianData.JobTechnicians
                .Where(jt => jt.Job.Status == JobStatus.Completed)
                .Select(jt => jt.Job)
                .ToList();

            if (!completedJobs.Any())
                return new List<RepairHistoryDto>();

            // Nhóm theo Vehicle (vì 1 xe có thể sửa nhiều lần)
            var grouped = completedJobs.GroupBy(j => j.RepairOrder.Vehicle.VehicleId);

            var result = grouped.Select(g =>
            {
                var vehicle = g.First().RepairOrder.Vehicle;
                var owner = vehicle.User;
                return new RepairHistoryDto
                {
                    VehicleModelId = vehicle.ModelId.ToString(),
                    LicensePlate = vehicle.LicensePlate,
                    VIN = vehicle.VIN,
                    OwnerFullName = owner.FullName,
                    OwnerPhone = owner.PhoneNumber,
                    OwnerEmail = owner.Email,
                    RepairCount = vehicle.RepairOrders?.Count() ?? 1,
                    CompletedJobs = g.Select(job => new JobHistoryDto
                    {
                        JobName = job.JobName,
                        Note = job.Note,
                        TotalAmount = job.TotalAmount,
                        Deadline = job.Deadline,
                        Level = job.Level,
                        CustomerIssue = job.RepairOrder.Note,
                        JobParts = job.JobParts.Select(p => new JobPartDto
                        {
                            PartName = p.Part.Name,
                            Quantity = p.Quantity,
                            UnitPrice = p.UnitPrice
                        }).ToList(),
                        Services = job.RepairOrder.RepairOrderServices?.Select(s => new ServiceDto
                        {
                            ServiceName = s.Service.ServiceName,
                            ServicePrice = s.ServicePrice,
                            ActualDuration = s.ActualDuration,
                            Notes = s.Notes
                        }).ToList() ?? new List<ServiceDto>()
                    }).ToList()
                };
            }).ToList();

            return result;
        }
    }
}
