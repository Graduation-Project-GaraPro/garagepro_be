using AutoMapper;
using BusinessObject.InspectionAndRepair;
using BusinessObject;
using Dtos.InspectionAndRepair;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BusinessObject.Vehicles;
using BusinessObject.Authentication;

namespace Garage_pro_api.Mapper
{
    public class RepairMappingProfile : Profile
    {
        public RepairMappingProfile()
        {
            // Map RepairCreateDto -> Repair
            CreateMap<RepairCreateDto, Repair>()
                .ForMember(dest => dest.EstimatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.RepairId, opt => opt.Ignore())
                .ForMember(dest => dest.StartTime, opt => opt.Ignore())
                .ForMember(dest => dest.EndTime, opt => opt.Ignore())
                .ForMember(dest => dest.ActualTime, opt => opt.Ignore())
                .ForMember(dest => dest.Job, opt => opt.Ignore())
                .ForMember(dest => dest.EstimatedTimeTicks, opt => opt.Ignore())
                .ForMember(dest => dest.ActualTimeTicks, opt => opt.Ignore());

            // Map RepairUpdateDto -> Repair (partial update)
            CreateMap<RepairUpdateDto, Repair>()
                 .ForMember(dest => dest.Description,
                     opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Description)))
                 .ForMember(dest => dest.Notes,
                     opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Notes)))
                 .ForMember(dest => dest.RepairId, opt => opt.Ignore())
                 .ForMember(dest => dest.JobId, opt => opt.Ignore())
                 .ForMember(dest => dest.StartTime, opt => opt.Ignore())
                 .ForMember(dest => dest.EndTime, opt => opt.Ignore())
                 .ForMember(dest => dest.ActualTime, opt => opt.Ignore())
                 .ForMember(dest => dest.EstimatedTime, opt => opt.Ignore())
                 .ForMember(dest => dest.Job, opt => opt.Ignore());

            // Map Repair -> RepairResponseDto
            CreateMap<Repair, RepairResponseDto>()
                .ForMember(dest => dest.RepairOrderId, opt => opt.MapFrom(src =>
                    src.Job != null ? src.Job.RepairOrderId : Guid.Empty))
                .ForMember(dest => dest.JobName, opt => opt.MapFrom(src =>
                    src.Job != null ? src.Job.JobName : string.Empty))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src =>
                    src.Job != null && src.Job.Service != null ? src.Job.Service.ServiceName : string.Empty))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes ?? string.Empty))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                // FIX: Map explicitly from Ticks properties
                .ForMember(dest => dest.ActualTime, opt => opt.MapFrom(src =>
                    src.ActualTimeTicks.HasValue ? TimeSpan.FromTicks(src.ActualTimeTicks.Value) : (TimeSpan?)null))
                .ForMember(dest => dest.EstimatedTime, opt => opt.MapFrom(src =>
                    src.EstimatedTimeTicks.HasValue ? TimeSpan.FromTicks(src.EstimatedTimeTicks.Value) : (TimeSpan?)null));

            // Map Repair -> RepairDto
            CreateMap<Repair, RepairDto>()
                .ForMember(dest => dest.RepairId, opt => opt.MapFrom(src => src.RepairId))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes ?? string.Empty))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                // FIX: Map explicitly from Ticks properties
                .ForMember(dest => dest.ActualTime, opt => opt.MapFrom(src =>
                    src.ActualTimeTicks.HasValue ? TimeSpan.FromTicks(src.ActualTimeTicks.Value) : (TimeSpan?)null))
                .ForMember(dest => dest.EstimatedTime, opt => opt.MapFrom(src =>
                    src.EstimatedTimeTicks.HasValue ? TimeSpan.FromTicks(src.EstimatedTimeTicks.Value) : (TimeSpan?)null));

            // Map JobPart -> JobPartDto
            CreateMap<JobPart, JobPartDto>()
                .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => src.PartId))
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src =>
                    src.Part != null ? src.Part.Name : string.Empty))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src =>
                    src.Part != null ? src.Part.Price : 0));

            // Map Job -> JobDetailDto
            CreateMap<Job, JobDetailDto>()
                .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.JobId))
                .ForMember(dest => dest.JobName, opt => opt.MapFrom(src => src.JobName ?? string.Empty))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src =>
                    src.Service != null ? src.Service.ServiceName : string.Empty))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note ?? string.Empty))
                .ForMember(dest => dest.Parts, opt => opt.MapFrom(src =>
                    src.JobParts != null
                        ? src.JobParts
                            .Where(jp => jp.Part != null && jp.Part.PartCategory != null)
                            .GroupBy(jp => new
                            {
                                jp.Part.PartCategoryId,
                                jp.Part.PartCategory.CategoryName
                            })
                            .Select(g => new PartCategoryRepairDto
                            {
                                PartCategoryId = g.Key.PartCategoryId,
                                CategoryName = g.Key.CategoryName,
                                Parts = g.Select(jp => new JobPartDto
                                {
                                    PartId = jp.Part.PartId,
                                    PartName = jp.Part.Name ?? string.Empty,
                                    UnitPrice = jp.Part.Price
                                }).ToList()
                            }).ToList()
                        : new List<PartCategoryRepairDto>()
                ))
                .ForMember(dest => dest.Repairs, opt => opt.MapFrom(src => src.Repair))
                .ForMember(dest => dest.Technicians, opt => opt.MapFrom(src =>
                    src.JobTechnicians != null
                        ? src.JobTechnicians
                            .Where(jt => jt.Technician != null)
                            .Select(jt => jt.Technician)
                            .ToList()
                        : new List<Technician>()));

            // Map VehicleBrand -> VehicleBrandDto
            CreateMap<VehicleBrand, VehicleBrandDto>()
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.BrandID))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.BrandName ?? string.Empty))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country ?? string.Empty));

            // Map VehicleModel -> VehicleModelDto
            CreateMap<VehicleModel, VehicleModelDto>()
                .ForMember(dest => dest.ModelId, opt => opt.MapFrom(src => src.ModelID))
                .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.ModelName ?? string.Empty))
                .ForMember(dest => dest.ManufacturingYear, opt => opt.MapFrom(src => src.ManufacturingYear));

            // Map Vehicle -> VehicleDto
            CreateMap<Vehicle, VehicleDto>()
                .ForMember(dest => dest.VehicleId, opt => opt.MapFrom(src => src.VehicleId))
                .ForMember(dest => dest.LicensePlate, opt => opt.MapFrom(src => src.LicensePlate ?? string.Empty))
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN ?? string.Empty))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model));

            // Map ApplicationUser -> CustomerDto
            CreateMap<ApplicationUser, CustomerDto>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    $"{src.FirstName ?? string.Empty} {src.LastName ?? string.Empty}".Trim()));

            // Map Technician -> TechnicianDto
            CreateMap<Technician, TechnicianDto>()
                .ForMember(dest => dest.TechnicianId, opt => opt.MapFrom(src => src.TechnicianId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    src.User != null
                        ? $"{src.User.FirstName ?? string.Empty} {src.User.LastName ?? string.Empty}".Trim()
                        : string.Empty))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.PhoneNumber : string.Empty));

            // Map RepairOrder -> RepairDetailDto
            CreateMap<RepairOrder, RepairDetailDto>()
                .ForMember(dest => dest.RepairOrderId, opt => opt.MapFrom(src => src.RepairOrderId))
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src =>
                    src.Vehicle != null ? src.Vehicle.VIN : string.Empty))
                .ForMember(dest => dest.VehicleLicensePlate, opt => opt.MapFrom(src =>
                    src.Vehicle != null ? src.Vehicle.LicensePlate : string.Empty))
                .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.User != null
                        ? $"{src.User.FirstName ?? string.Empty} {src.User.LastName ?? string.Empty}".Trim()
                        : string.Empty))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.PhoneNumber : string.Empty))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note ?? string.Empty))
                .ForMember(dest => dest.Jobs, opt => opt.MapFrom(src =>
                    src.Jobs != null ? src.Jobs.ToList() : new List<Job>()));
        }
    }
}