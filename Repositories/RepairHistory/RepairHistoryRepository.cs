using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Branches;
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;
using BusinessObject.Vehicles;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.RepairHistory
{
    public class RepairHistoryRepository : IRepairHistoryRepository
    {
        private readonly MyAppDbContext _context;

        public RepairHistoryRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<Technician> GetTechnicianByUserIdAsync(string userId)
        {
            return await _context.Technicians
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task<Technician> GetTechnicianWithCompletedJobsAsync(Guid technicianId)
        {
            // Bước 1: Lấy danh sách JobIds completed
            var completedJobIds = await _context.JobTechnicians
                .AsNoTracking()
                .Where(jt => jt.TechnicianId == technicianId &&
                             jt.Job.Status == JobStatus.Completed)
                .Select(jt => jt.JobId)
                .Distinct()
                .ToListAsync();

            if (!completedJobIds.Any())
            {
                // Return empty technician nếu không có jobs
                return new Technician
                {
                    TechnicianId = technicianId,
                    JobTechnicians = new List<JobTechnician>()
                };
            }

            // Bước 2: Load Jobs với Projection
            var jobs = await _context.Jobs
                .AsNoTracking()
                .Where(j => completedJobIds.Contains(j.JobId))
                .Select(j => new Job
                {
                    JobId = j.JobId,
                    JobName = j.JobName,
                    Status = j.Status,
                    TotalAmount = j.TotalAmount,
                    Deadline = j.Deadline,
                    Note = j.Note,
                    Level = j.Level,
                    ServiceId = j.ServiceId,
                    RepairOrderId = j.RepairOrderId,

                    Repair = j.Repair == null ? null : new Repair
                    {
                        RepairId = j.Repair.RepairId,
                        Description = j.Repair.Description
                    },

                    RepairOrder = new RepairOrder
                    {
                        RepairOrderId = j.RepairOrder.RepairOrderId,
                        Note = j.RepairOrder.Note,
                        VehicleId = j.RepairOrder.VehicleId,

                        Vehicle = new Vehicle
                        {
                            VehicleId = j.RepairOrder.Vehicle.VehicleId,
                            LicensePlate = j.RepairOrder.Vehicle.LicensePlate,
                            VIN = j.RepairOrder.Vehicle.VIN,
                            UserId = j.RepairOrder.Vehicle.UserId,

                            Brand = j.RepairOrder.Vehicle.Brand == null ? null : new VehicleBrand
                            {
                                BrandID = j.RepairOrder.Vehicle.Brand.BrandID,
                                BrandName = j.RepairOrder.Vehicle.Brand.BrandName
                            },

                            Model = j.RepairOrder.Vehicle.Model == null ? null : new VehicleModel
                            {
                                ModelID = j.RepairOrder.Vehicle.Model.ModelID,
                                ModelName = j.RepairOrder.Vehicle.Model.ModelName,
                                ManufacturingYear = j.RepairOrder.Vehicle.Model.ManufacturingYear
                            },

                            User = new ApplicationUser
                            {
                                Id = j.RepairOrder.Vehicle.User.Id,
                                FirstName = j.RepairOrder.Vehicle.User.FirstName,
                                LastName = j.RepairOrder.Vehicle.User.LastName,
                                Email = j.RepairOrder.Vehicle.User.Email,
                                PhoneNumber = j.RepairOrder.Vehicle.User.PhoneNumber
                            }
                        }
                    }
                })
                .ToListAsync();

            // Bước 3: Load JobParts riêng
            var jobParts = await _context.JobParts
                .AsNoTracking()
                .Where(jp => completedJobIds.Contains(jp.JobId))
                .Select(jp => new
                {
                    jp.JobId,
                    PartName = jp.Part.Name,
                    jp.Quantity,
                    jp.UnitPrice
                })
                .ToListAsync();

            // Bước 4: Load RepairOrderServices riêng
            var repairOrderIds = jobs.Select(j => j.RepairOrderId).Distinct().ToList();
            var repairOrderServices = await _context.RepairOrderServices
                .AsNoTracking()
                .Where(ros => repairOrderIds.Contains(ros.RepairOrderId))
                .Select(ros => new
                {
                    ros.RepairOrderId,
                    ros.ServiceId,
                    ServiceName = ros.Service.ServiceName,
                    ros.ActualDuration,
                    ros.Notes
                })
                .ToListAsync();

            // Bước 5: Count RepairOrders per Vehicle
            var vehicleIds = jobs.Select(j => j.RepairOrder.Vehicle.VehicleId).Distinct().ToList();
            var repairOrderCounts = await _context.RepairOrders
                .AsNoTracking()
                .Where(ro => vehicleIds.Contains(ro.VehicleId))
                .GroupBy(ro => ro.VehicleId)
                .Select(g => new { VehicleId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.VehicleId, x => x.Count);

            // Bước 6: Gán collections vào jobs
            foreach (var job in jobs)
            {
                // Gán JobParts
                job.JobParts = jobParts
                    .Where(jp => jp.JobId == job.JobId)
                    .Select(jp => new JobPart
                    {
                        JobId = jp.JobId,
                        Part = new Part
                        {
                            Name = jp.PartName
                        },
                        Quantity = jp.Quantity,
                        UnitPrice = jp.UnitPrice
                    })
                    .ToList();

                // Gán RepairOrderServices
                job.RepairOrder.RepairOrderServices = repairOrderServices
                    .Where(ros => ros.RepairOrderId == job.RepairOrderId)
                    .Select(ros => new RepairOrderService
                    {
                        RepairOrderId = ros.RepairOrderId,
                        ServiceId = ros.ServiceId,
                        Service = new Service
                        {
                            ServiceName = ros.ServiceName
                        },
                        ActualDuration = ros.ActualDuration,
                        Notes = ros.Notes
                    })
                    .ToList();

                // Gán RepairOrders count
                if (repairOrderCounts.TryGetValue(job.RepairOrder.Vehicle.VehicleId, out var count))
                {
                    job.RepairOrder.Vehicle.RepairOrders = Enumerable
                        .Range(0, count)
                        .Select(_ => new RepairOrder())
                        .ToList();
                }
            }

            // Return Technician với JobTechnicians
            return new Technician
            {
                TechnicianId = technicianId,
                JobTechnicians = jobs.Select(j => new JobTechnician
                {
                    TechnicianId = technicianId,
                    JobId = j.JobId,
                    Job = j
                }).ToList()
            };
        }
    }
}