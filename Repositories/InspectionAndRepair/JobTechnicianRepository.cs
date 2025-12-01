using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Branches;
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.InspectionAndRepair
{
    public class JobTechnicianRepository : IJobTechnicianRepository
    {
        private readonly MyAppDbContext _context;

        public JobTechnicianRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Job>> GetJobsByTechnicianAsync(string userId)
        {
            var jobIds = await _context.JobTechnicians
                .AsNoTracking()
                .Where(jt => jt.Technician.UserId == userId)
                .Select(jt => jt.JobId)
                .Distinct()
                .ToListAsync();

            if (!jobIds.Any())
                return new List<Job>();

            var query = from j in _context.Jobs.AsNoTracking()
                        where jobIds.Contains(j.JobId)
                        select new Job
                        {
                            JobId = j.JobId,
                            JobName = j.JobName,
                            Status = j.Status,
                            Deadline = j.Deadline,
                            TotalAmount = j.TotalAmount,
                            Note = j.Note,
                            CreatedAt = j.CreatedAt,
                            UpdatedAt = j.UpdatedAt,
                            RepairOrderId = j.RepairOrderId,

                            Service = new Service
                            {
                                ServiceId = j.Service.ServiceId,
                                ServiceName = j.Service.ServiceName
                            },

                            RepairOrder = new RepairOrder
                            {
                                RepairOrderId = j.RepairOrder.RepairOrderId,
                                Vehicle = new Vehicle
                                {
                                    VehicleId = j.RepairOrder.Vehicle.VehicleId,
                                    LicensePlate = j.RepairOrder.Vehicle.LicensePlate,
                                    Year = j.RepairOrder.Vehicle.Year,
                                    Color = j.RepairOrder.Vehicle.Color,
                                    Brand = new BusinessObject.Vehicles.VehicleBrand
                                    {
                                        BrandID = j.RepairOrder.Vehicle.Brand.BrandID,
                                        BrandName = j.RepairOrder.Vehicle.Brand.BrandName
                                    },
                                    Model = new BusinessObject.Vehicles.VehicleModel
                                    {
                                        ModelID = j.RepairOrder.Vehicle.Model.ModelID,
                                        ModelName = j.RepairOrder.Vehicle.Model.ModelName
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
                            },

                            Repair = j.Repair == null ? null : new Repair
                            {
                                RepairId = j.Repair.RepairId,
                                Description = j.Repair.Description,
                                StartTime = j.Repair.StartTime,
                                EndTime = j.Repair.EndTime,
                                ActualTime = j.Repair.ActualTime,
                                EstimatedTime = j.Repair.EstimatedTime,
                                Notes = j.Repair.Notes,
                                JobId = j.Repair.JobId
                            }
                        };

            var jobs = await query.ToListAsync();

            if (jobs.Any())
            {
                var jobParts = await _context.JobParts
                    .AsNoTracking()
                    .Where(jp => jobIds.Contains(jp.JobId))
                    .Select(jp => new
                    {
                        jp.JobId,
                        Part = new Part
                        {
                            PartId = jp.Part.PartId,
                            Name = jp.Part.Name,
                            PartCategoryId = jp.Part.PartCategoryId,
                            PartCategory = new PartCategory
                            {
                                LaborCategoryId = jp.Part.PartCategory.LaborCategoryId,
                                CategoryName = jp.Part.PartCategory.CategoryName
                            }
                        }
                    })
                    .ToListAsync();

                var jobTechnicians = await _context.JobTechnicians
                    .AsNoTracking()
                    .Where(jt => jobIds.Contains(jt.JobId))
                    .Select(jt => new
                    {
                        jt.JobId,
                        Technician = new Technician
                        {
                            TechnicianId = jt.Technician.TechnicianId,
                            User = new ApplicationUser
                            {
                                Id = jt.Technician.User.Id,
                                FirstName = jt.Technician.User.FirstName,
                                LastName = jt.Technician.User.LastName,
                                Email = jt.Technician.User.Email,
                                PhoneNumber = jt.Technician.User.PhoneNumber
                            }
                        }
                    })
                    .ToListAsync();

                foreach (var job in jobs)
                {
                    job.JobParts = jobParts
                        .Where(jp => jp.JobId == job.JobId)
                        .Select(jp => new JobPart
                        {
                            JobId = jp.JobId,
                            Part = jp.Part
                        })
                        .ToList();

                    job.JobTechnicians = jobTechnicians
                        .Where(jt => jt.JobId == job.JobId)
                        .Select(jt => new JobTechnician
                        {
                            JobId = jt.JobId,
                            Technician = jt.Technician
                        })
                        .ToList();
                }
            }

            return jobs;
        }

        public async Task UpdateJobStatusAsync(Guid jobId, JobStatus newStatus, DateTime? endTime = null, TimeSpan? actualTime = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var job = await _context.Jobs.Include(j=>j.RepairOrder).FirstOrDefaultAsync(j => j.JobId == jobId);
                if (job == null)
                    throw new Exception("Job không tồn tại");

                job.Status = newStatus;
                job.UpdatedAt = DateTime.UtcNow;

                if (endTime.HasValue)
                {
                    var repair = await _context.Repairs.FirstOrDefaultAsync(r => r.JobId == jobId);
                    if (repair != null)
                    {
                        repair.EndTime = endTime;
                        repair.ActualTime = actualTime;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<Technician?> GetTechnicianByUserIdAsync(string userId)
        {
            return await _context.Technicians
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }
    }
}