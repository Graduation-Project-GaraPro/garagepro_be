﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Enums;
using BusinessObject;
using DataAccessLayer;
using Dtos.InspectionAndRepair;
using Dtos.RepairProgressDto;
using Microsoft.EntityFrameworkCore;

namespace Repositories.RepairProgressRepositories
{
    public class RepairProgressRepository : IRepairProgressRepository
    {

        private readonly MyAppDbContext _context;

        public RepairProgressRepository(MyAppDbContext context)
        {
            _context = context;
        }
        public async Task<PagedResult<RepairOrderListItemDto>> GetRepairOrdersByUserIdAsync(string userId, RepairOrderFilterDto filter)
        {
            var query = _context.RepairOrders
                .Include(rp => rp.OrderStatus).ThenInclude(os => os.Labels)
                .Include(rp => rp.Vehicle).ThenInclude(v => v.Brand)
                .Include(rp => rp.Vehicle).ThenInclude(v => v.Model)
                .Include(rp => rp.Jobs).ThenInclude(j => j.JobParts).ThenInclude(jp => jp.Part)
                .Include(rp => rp.Jobs).ThenInclude(j => j.JobTechnicians).ThenInclude(jt => jt.Technician).ThenInclude(t => t.User)
                .Include(rp => rp.Jobs).ThenInclude(j => j.Repair)
                .Include(rp => rp.User)
                .Where(ro => ro.UserId == userId && !ro.IsArchived)
                .AsQueryable();

            // Apply filters
            if (filter.StatusId.HasValue)
            {
                query = query.Where(ro => ro.StatusId == filter.StatusId.Value);
            }

            if (filter.RoType.HasValue)
            {
                query = query.Where(ro => ro.RoType == filter.RoType.Value);
            }

            if (!string.IsNullOrEmpty(filter.PaidStatus))
            {
                query = query.Where(ro => ro.PaidStatus == filter.PaidStatus);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(ro => ro.ReceiveDate >= filter.FromDate.Value.Date);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(ro => ro.ReceiveDate <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));
            }


            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .OrderByDescending(ro => ro.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(ro => new RepairOrderListItemDto
                {
                    RepairOrderId = ro.RepairOrderId,
                    ReceiveDate = ro.ReceiveDate,
                    RoType = ro.RoType.ToString(),
                    EstimatedCompletionDate = ro.EstimatedCompletionDate,
                    CompletionDate = ro.CompletionDate,
                    Cost = ro.Cost,
                    PaidStatus = ro.PaidStatus,
                    VehicleLicensePlate = ro.Vehicle.LicensePlate,
                    VehicleModel = ro.Vehicle.Model.ModelName,
                    StatusName = ro.OrderStatus.StatusName,
                    Labels = ro.OrderStatus.Labels.Select(l => new LabelDto
                    {
                        LabelId = l.LabelId,
                        LabelName = l.LabelName,
                        Description = l.Description,
                        ColorName = l.ColorName,
                        HexCode = l.HexCode
                    }).ToList(),
                    ProgressPercentage = CalculateProgressPercentage(ro.Jobs),
                    ProgressStatus = GetProgressStatus(ro.Jobs)
                })
                .ToListAsync();

            return new PagedResult<RepairOrderListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<RepairOrderProgressDto?> GetRepairOrderProgressAsync(Guid repairOrderId, string userId)
        {
            var repairOrder = await _context.RepairOrders
                .Include(rp=>rp.OrderStatus).ThenInclude(os=>os.Labels)
                .Include(rp=>rp.Vehicle).ThenInclude(v=>v.Brand)
                .Include(rp => rp.Vehicle).ThenInclude(v => v.Model)
                .Include(rp=>rp.Jobs).ThenInclude(j=>j.JobParts).ThenInclude(jp=>jp.Part)
                .Include(rp => rp.Jobs).ThenInclude(j=>j.JobTechnicians).ThenInclude(jt=>jt.Technician).ThenInclude(t=>t.User)
                .Include(rp => rp.Jobs).ThenInclude(j=>j.Repair)
                .Include(rp=>rp.User)
                

                .Where(ro => ro.RepairOrderId == repairOrderId &&
                            ro.UserId == userId &&
                            !ro.IsArchived)
                .Select(ro => new RepairOrderProgressDto
                {
                    RepairOrderId = ro.RepairOrderId,
                    ReceiveDate = ro.ReceiveDate,
                    RoType = ro.RoType.ToString(),
                    EstimatedCompletionDate = ro.EstimatedCompletionDate,
                    CompletionDate = ro.CompletionDate,
                    Cost = ro.Cost,
                    EstimatedAmount = ro.EstimatedAmount,
                    PaidAmount = ro.PaidAmount,
                    PaidStatus = ro.PaidStatus,
                    Note = ro.Note ?? string.Empty,
                    Vehicle = new Dtos.RepairProgressDto.VehicleDto
                    {
                        VehicleId = ro.Vehicle.VehicleId,
                        LicensePlate = ro.Vehicle.LicensePlate,
                        Model = ro.Vehicle.Model.ModelName ,
                        Brand = ro.Vehicle.Brand.BrandName,
                        Year = ro.Vehicle.Year
                    },
                    OrderStatus = new OrderStatusDto
                    {
                        OrderStatusId = ro.OrderStatus.OrderStatusId,
                        StatusName = ro.OrderStatus.StatusName,
                        Labels = ro.OrderStatus.Labels.Select(l => new LabelDto
                        {
                            LabelId = l.LabelId,
                            LabelName = l.LabelName,
                            Description = l.Description,
                            ColorName = l.ColorName,
                            HexCode = l.HexCode
                        }).ToList()
                    },
                    Jobs = ro.Jobs.Select(j => new JobDto
                    {
                        JobId = j.JobId,
                        JobName = j.JobName,
                        Status = j.Status.ToString(),
                        Deadline = j.Deadline,
                        TotalAmount = j.TotalAmount,
                        Note = j.Note ?? string.Empty,
                        Level = j.Level,
                        Repair = j.Repair != null ? new Dtos.RepairProgressDto.RepairDto
                        {
                            RepairId = j.Repair.RepairId,
                            Description = j.Repair.Description ?? string.Empty,
                            StartTime = j.Repair.StartTime,
                            EndTime = j.Repair.EndTime,
                            ActualTime = j.Repair.ActualTime,
                            EstimatedTime = j.Repair.EstimatedTime,
                            Notes = j.Repair.Notes ?? string.Empty
                        } : null,
                        Parts = j.JobParts.Select(jp => new Dtos.RepairProgressDto.PartDto
                        {
                            PartId = jp.Part.PartId,
                            Name = jp.Part.Name,
                            Price = jp.Part.Price
                                                 
                        }).ToList(),
                        Technicians = j.JobTechnicians.Select(jt => new Dtos.RepairProgressDto.TechnicianDto
                        {
                            TechnicianId = jt.Technician.TechnicianId,
                            FullName = jt.Technician.User.LastName + " "+ jt.Technician.User.FirstName,
                            Email = jt.Technician.User.Email,
                            PhoneNumber = jt.Technician.User.PhoneNumber
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return repairOrder;
        }

        public async Task<bool> IsRepairOrderAccessibleByUserAsync(Guid repairOrderId, string userId)
        {
            return await _context.RepairOrders
                .AnyAsync(ro => ro.RepairOrderId == repairOrderId &&
                               ro.UserId == userId &&
                               !ro.IsArchived);
        }

        // Helper methods for progress calculation
        private static decimal CalculateProgressPercentage(ICollection<Job> jobs)
        {
            if (jobs == null || !jobs.Any()) return 0;

            var completedJobs = jobs.Count(j => j.Status == JobStatus.Completed);
            return (decimal)completedJobs / jobs.Count * 100;
        }

        private static string GetProgressStatus(ICollection<Job> jobs)
        {
            if (jobs == null || !jobs.Any()) return "Chưa bắt đầu";

            var completedJobs = jobs.Count(j => j.Status == JobStatus.Completed);
            var percentage = (decimal)completedJobs / jobs.Count * 100;

            return percentage switch
            {
                0 => "Chưa bắt đầu",
                < 100 => "Đang thực hiện",
                100 => "Hoàn thành",
                _ => "Không xác định"
            };
        }

    }
}
