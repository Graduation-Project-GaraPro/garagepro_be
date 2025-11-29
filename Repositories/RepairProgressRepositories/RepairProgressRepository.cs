using System;
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
using AutoMapper.QueryableExtensions;
using AutoMapper;
using Dtos.RepairOrderArchivedDtos;

namespace Repositories.RepairProgressRepositories
{
    public class RepairProgressRepository : IRepairProgressRepository
    {

        private readonly MyAppDbContext _context;

        public RepairProgressRepository(MyAppDbContext context)
        {
            _context = context;
        }
        public async Task<PagedResult<RepairOrderListItemDto>> GetRepairOrdersByUserIdAsync(
    string userId,
    RepairOrderFilterDto filter)
        {
            // 1. Base query WITHOUT includes
            var baseQuery = _context.RepairOrders
                .Where(ro => ro.UserId == userId && !ro.IsArchived);

            // Apply filters
            if (filter.StatusId.HasValue)
            {
                baseQuery = baseQuery.Where(ro => ro.StatusId == filter.StatusId.Value);
            }

            if (filter.RoType.HasValue)
            {
                baseQuery = baseQuery.Where(ro => ro.RoType == filter.RoType.Value);
            }

            if (!string.IsNullOrEmpty(filter.PaidStatus))
            {
                // Assume PaidStatus is an enum
                if (Enum.TryParse<PaidStatus>(filter.PaidStatus, out var paidStatus))
                {
                    baseQuery = baseQuery.Where(ro => ro.PaidStatus == paidStatus);
                }
                // else ignore invalid value or handle as you want
            }

            if (filter.FromDate.HasValue)
            {
                var from = filter.FromDate.Value.Date;
                baseQuery = baseQuery.Where(ro => ro.ReceiveDate >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var to = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                baseQuery = baseQuery.Where(ro => ro.ReceiveDate <= to);
            }

            // 2. Get total count using the LIGHT query
            var totalCount = await baseQuery.CountAsync();

            // 3. Get the page of IDs first (still light)
            var pageQuery = baseQuery
                .OrderByDescending(ro => ro.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize);

            var repairOrderIds = await pageQuery
                .Select(ro => ro.RepairOrderId)
                .ToListAsync();

            // 4. Heavy query only on the paged results
            var itemsQuery = _context.RepairOrders
                .AsNoTracking()
                .Where(ro => repairOrderIds.Contains(ro.RepairOrderId))
                .Include(rp => rp.OrderStatus).ThenInclude(os => os.Labels)
                .Include(rp => rp.Vehicle).ThenInclude(v => v.Brand)
                .Include(rp => rp.Vehicle).ThenInclude(v => v.Model)
                .Include(rp => rp.Jobs).ThenInclude(j => j.JobParts).ThenInclude(jp => jp.Part)
                .Include(rp => rp.Jobs).ThenInclude(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician).ThenInclude(t => t.User)
                .Include(rp => rp.Jobs).ThenInclude(j => j.Repair)
                .Include(rp => rp.User)
                .Include(rp => rp.FeedBack)
                .AsSplitQuery(); // EF Core 5+ – VERY helpful for this graph

            var items = await itemsQuery
                .OrderByDescending(ro => ro.CreatedAt) // keep ordering
                .Select(ro => new RepairOrderListItemDto
                {
                    RepairOrderId = ro.RepairOrderId,
                    ReceiveDate = ro.ReceiveDate,
                    RoType = ro.RoType.ToString(),
                    EstimatedCompletionDate = ro.EstimatedCompletionDate,
                    CompletionDate = ro.CompletionDate,
                    Cost = ro.Cost,
                    PaidStatus = ro.PaidStatus.ToString(),
                    VehicleLicensePlate = ro.Vehicle.LicensePlate,
                    VehicleModel = ro.Vehicle.Model.ModelName,
                    StatusName = ro.OrderStatus.StatusName,
                    IsArchived = ro.IsArchived,
                    ArchivedAt = ro.ArchivedAt,
                    Labels = ro.OrderStatus.Labels.Select(l => new LabelDto
                    {
                        LabelId = l.LabelId,
                        LabelName = l.LabelName,
                        Description = l.Description,
                        ColorName = l.ColorName,
                        HexCode = l.HexCode
                    }).ToList(),
                    ProgressPercentage = CalculateProgressPercentage(ro.Jobs),
                    ProgressStatus = GetProgressStatus(ro.Jobs),
                    FeedBacks = ro.FeedBack != null ? new FeedbackDto
                    {
                        Rating = ro.FeedBack.Rating,
                        Description = ro.FeedBack.Description ?? string.Empty,
                        CreatedAt = ro.FeedBack.CreatedAt
                    } : null
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


        public async Task<PagedResult<RepairOrderArchivedListItemDto>> GetArchivedRepairOrdersByUserIdAsync(
            string userId,
            RepairOrderFilterDto filter,
            IMapper mapper)
        {
            var query = _context.RepairOrders
                .AsNoTracking()
                .Where(ro => ro.UserId == userId
                             && ro.StatusId == 3          // completed
                             && ro.IsArchived)            // archived
                .Include(ro => ro.Branch)
                .Include(ro => ro.Vehicle)
                    .ThenInclude(v => v.Brand)
                .Include(ro => ro.Vehicle)
                    .ThenInclude(v => v.Model)
                .AsQueryable();

            // Apply thêm filter (date, branch, vehicle...) nếu bạn có trong RepairOrderFilterDto

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
                query = query.Where(ro => ro.PaidStatus.ToString() == filter.PaidStatus);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(ro => ro.ReceiveDate >= filter.FromDate.Value.Date);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(ro => ro.ReceiveDate <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(ro => ro.ArchivedAt ?? ro.CompletionDate ?? ro.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ProjectTo<RepairOrderArchivedListItemDto>(mapper.ConfigurationProvider)
                .ToListAsync();

            return new PagedResult<RepairOrderArchivedListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<RepairOrderArchivedDetailDto?> GetArchivedRepairOrderDetailAsync(
            Guid repairOrderId,
            string userId,
            IMapper mapper)
        {
            var query = _context.RepairOrders
                .AsNoTracking()
                .Where(ro => ro.RepairOrderId == repairOrderId
                             && ro.UserId == userId
                             && ro.StatusId == 3
                             && ro.IsArchived)
                .Include(ro => ro.FeedBack)
                .Include(ro => ro.Branch)
                .Include(ro => ro.Vehicle)
                    .ThenInclude(v => v.Model)
                .Include(ro => ro.Vehicle)
                    .ThenInclude(v => v.Brand)
                .Include(ro => ro.Jobs)
                    .ThenInclude(j => j.Repair)
                .Include(ro => ro.Jobs)
                    .ThenInclude(j => j.JobTechnicians)
                        .ThenInclude(jt => jt.Technician)
                            .ThenInclude(t => t.User)
                .Include(ro => ro.Jobs)
                    .ThenInclude(j => j.JobParts)
                        .ThenInclude(jp => jp.Part);

            var entity = await query.FirstOrDefaultAsync();
            if (entity == null) return null;

            return mapper.Map<RepairOrderArchivedDetailDto>(entity);
        }


        public async Task<RepairOrderProgressDto?> GetRepairOrderProgressAsync(Guid repairOrderId, string userId)
        {
            var repairOrder = await _context.RepairOrders
                .AsNoTracking()
                .AsSplitQuery()
                .Include(rp => rp.OrderStatus).ThenInclude(os => os.Labels)
                .Include(rp => rp.Vehicle).ThenInclude(v => v.Brand)
                .Include(rp => rp.Vehicle).ThenInclude(v => v.Model)
                .Include(rp => rp.Jobs).ThenInclude(j => j.JobParts).ThenInclude(jp => jp.Part)
                .Include(rp => rp.Jobs).ThenInclude(j => j.JobTechnicians).ThenInclude(jt => jt.Technician).ThenInclude(t => t.User)
                .Include(rp => rp.Jobs).ThenInclude(j => j.Repair)
                .Include(rp => rp.User)
                .Include(rp => rp.FeedBack)


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
                    PaidStatus = ro.PaidStatus.ToString(),
                    IsArchived = ro.IsArchived,
                    ArchivedAt = ro.ArchivedAt,
                    Note = ro.Note ?? string.Empty,
                    
                    Vehicle = new Dtos.RepairProgressDto.VehicleDto
                    {
                        VehicleId = ro.Vehicle.VehicleId,
                        LicensePlate = ro.Vehicle.LicensePlate,
                        Model = ro.Vehicle.Model.ModelName,
                        Brand = ro.Vehicle.Brand.BrandName,
                        Year = ro.Vehicle.Year
                    },
                    FeedBacks = ro.FeedBack != null ? new FeedbackDto
                    {
                        Rating = ro.FeedBack.Rating,
                        Description = ro.FeedBack.Description ?? string.Empty,
                        CreatedAt = ro.FeedBack.CreatedAt
                    } : null,
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
                            FirstName = jt.Technician.User.FirstName,
                            LastName = jt.Technician.User.LastName,
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

            var value = (decimal)completedJobs / jobs.Count * 100;

            // Làm tròn về số chẵn
            return Math.Round(value, 0, MidpointRounding.ToEven);
        }

        private static string GetProgressStatus(ICollection<Job> jobs)
        {
            if (jobs == null || !jobs.Any()) return "Not Started";

            var completedJobs = jobs.Count(j => j.Status == JobStatus.Completed);
            var percentage = (decimal)completedJobs / jobs.Count * 100;

            return percentage switch
            {
                0 => "Not Started",
                < 100 => "In Progress",
                100 => "Completed",
                _ => "Unknown"
            };
        }

    }
}
