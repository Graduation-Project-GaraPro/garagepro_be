using DataAccessLayer;
using Dtos.Job;
using Dtos.RepairOrder;
using Dtos.Revenue;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Revenue
{
    public class AdminRepairOrderRepository: IAdminRepairOrderRepository
    {
        private readonly MyAppDbContext _context;

        public AdminRepairOrderRepository(MyAppDbContext context)
        {
            _context = context;
            // temporary increase command timeout for heavy queries (remove or reduce in prod)
            _context.Database.SetCommandTimeout(180);
        }

        public async Task<List<RepairOrderListItemDto>> GetRepairOrdersForListAsync(
            DateTime? startDate = null, DateTime? endDate = null, int page = 0, int pageSize = 50)
        {
            if (page < 0) page = 0;
            if (pageSize <= 0) pageSize = 50;

            // Normalize endDate to inclusive end-of-day if provided
            if (startDate.HasValue && !endDate.HasValue)
                endDate = startDate.Value.Date.AddDays(1).AddTicks(-1);
            else if (endDate.HasValue)
                endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);

            var q = _context.RepairOrders
                .AsNoTracking()
                .Where(r =>
                    !startDate.HasValue ||
                    ((r.CompletionDate != null && r.CompletionDate >= startDate && r.CompletionDate <= endDate)
                     || (r.CompletionDate == null && r.ReceiveDate != null && r.ReceiveDate >= startDate && r.ReceiveDate <= endDate))
                )
                .OrderByDescending(r => r.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(r => new RepairOrderListItemDto
                {
                    RepairOrderId = r.RepairOrderId,
                    CreatedAt = r.CreatedAt,
                    ReceiveDate = r.ReceiveDate,
                    CompletionDate = r.CompletionDate,

                    BranchId = r.Branch.BranchId,
                    BranchName = r.Branch.BranchName,
                    StatusId = r.StatusId,
                    StatusName = r.OrderStatus != null ? r.OrderStatus.StatusName : null,
                    LabelName = r.OrderStatus != null && r.OrderStatus.Labels.Any() ? r.OrderStatus.Labels.FirstOrDefault().LabelName : null,

                    CustomerName = r.User != null ? (r.User.FirstName + " " + r.User.LastName).Trim() : null,
                    CustomerPhone = r.User != null ? r.User.PhoneNumber : null,

                    LicensePlate = r.Vehicle != null ? r.Vehicle.LicensePlate : null,
                    VehicleVIN = r.Vehicle != null ? r.Vehicle.VIN : null,

                    EstimatedAmount = r.EstimatedAmount,
                    Cost = r.Cost,
                    PaidAmount = r.PaidAmount,
                    PaidStatus = r.PaidStatus,

                    JobCount = r.Jobs.Count(),
                    JobsTotalAmount = r.Jobs.Sum(j => (decimal?)j.TotalAmount) ?? 0m,
                    PartCount = r.Jobs.SelectMany(j => j.JobParts).Select(jp => jp.PartId).Distinct().Count(),

                    TopServiceNames = r.Jobs
                                        .Where(j => j.Service != null)
                                        .OrderByDescending(j => j.TotalAmount)
                                        .Take(3)
                                        .Select(j => j.Service.ServiceName)
                                        .ToList()
                });

            return await q.ToListAsync();
        }

        public async Task<List<RepairOrderSummaryDto>> GetRepairOrderSummariesAsync(
            DateTime start, DateTime end, Guid? branchId = null, Guid? technicianId = null, string serviceType = null)
        {
            var q = _context.RepairOrders
                .AsNoTracking()
                .Where(r => r.CompletionDate != null && r.CompletionDate >= start && r.CompletionDate <= end);

            if (branchId.HasValue)
                q = q.Where(r => r.BranchId == branchId.Value);

            if (technicianId.HasValue)
            {
                q = q.Where(r => r.Jobs.Any(j => j.JobTechnicians.Any(jt => jt.TechnicianId == technicianId.Value)));
            }

            if (!string.IsNullOrWhiteSpace(serviceType))
            {
                var svc = serviceType.Trim();
                q = q.Where(r => r.Jobs.Any(j => j.Service != null && EF.Functions.Like(j.Service.ServiceName, $"%{svc}%")));
            }

            return await q.Select(r => new RepairOrderSummaryDto
            {
                RepairOrderId = r.RepairOrderId,
                CompletionDate = r.CompletionDate,
                PaidAmount = r.PaidAmount,
                EstimatedAmount = r.EstimatedAmount,
                StatusId = r.StatusId,
                BranchId = r.BranchId,
                PaidStatus = r.PaidStatus
            }).ToListAsync();
        }

        public async Task<List<JobSummaryDto>> GetJobSummariesByCompletionDateRangeAsync(
            DateTime start, DateTime end, Guid? branchId = null, Guid? technicianId = null, string serviceType = null)
        {
            var q = _context.Jobs
                .AsNoTracking()
                .Where(j => j.RepairOrder != null
                            && j.RepairOrder.CompletionDate != null
                            && j.RepairOrder.CompletionDate >= start
                            && j.RepairOrder.CompletionDate <= end);

            if (branchId.HasValue)
                q = q.Where(j => j.RepairOrder.BranchId == branchId.Value);

            if (technicianId.HasValue)
                q = q.Where(j => j.JobTechnicians.Any(jt => jt.TechnicianId == technicianId.Value));

            if (!string.IsNullOrWhiteSpace(serviceType))
            {
                var svc = serviceType.Trim();
                q = q.Where(j => j.Service != null && EF.Functions.Like(j.Service.ServiceName, $"%{svc}%"));
            }

            return await q.Select(j => new JobSummaryDto
            {
                JobId = j.JobId,
                RepairOrderId = j.RepairOrderId,
                ServiceId = j.ServiceId,
                ServiceName = j.Service != null ? j.Service.ServiceName : null,
                TotalAmount = j.TotalAmount,
                CreatedAt = j.CreatedAt
            }).ToListAsync();
        }

        public async Task<List<JobSummaryDto>> GetJobSummariesByRepairOrderIdsAsync(IEnumerable<Guid> repairOrderIds)
        {
            var ids = repairOrderIds as Guid[] ?? repairOrderIds.ToArray();
            if (!ids.Any()) return new List<JobSummaryDto>();

            return await _context.Jobs
                .AsNoTracking()
                .Where(j => ids.Contains(j.RepairOrderId))
                .Select(j => new JobSummaryDto
                {
                    JobId = j.JobId,
                    RepairOrderId = j.RepairOrderId,
                    ServiceId = j.ServiceId,
                    ServiceName = j.Service != null ? j.Service.ServiceName : null,
                    TotalAmount = j.TotalAmount,
                    CreatedAt = j.CreatedAt
                }).ToListAsync();
        }
    }
}
