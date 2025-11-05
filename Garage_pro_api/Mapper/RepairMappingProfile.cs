using AutoMapper;
using BusinessObject.InspectionAndRepair;
using BusinessObject;
using Dtos.InspectionAndRepair;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BusinessObject.Vehicles;
using BusinessObject.Authentication;
using Dtos.Emergency;
using BusinessObject.RequestEmergency;

namespace Garage_pro_api.Mapper
{
    public class RepairMappingProfile : Profile
    {
        public RepairMappingProfile()
        {
            CreateMap<RepairCreateDto, Repair>();

            CreateMap<JobPart, JobPartDto>()
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.Part.Name))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Part.Price));

            CreateMap<Repair, RepairResponseDto>()
                .ForMember(dest => dest.JobName, opt => opt.MapFrom(src => src.Job.JobName))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Job.Service.ServiceName));

            CreateMap<Repair, RepairDto>()
                .ForMember(dest => dest.RepairId, opt => opt.MapFrom(src => src.RepairId))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                .ForMember(dest => dest.ActualTime, opt => opt.MapFrom(src => src.ActualTime))
                .ForMember(dest => dest.EstimatedTime, opt => opt.MapFrom(src => src.EstimatedTime));

            CreateMap<Job, JobDetailDto>()
                .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.JobId))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.ServiceName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Parts, opt => opt.MapFrom(src => src.JobParts))
                .ForMember(dest => dest.Repairs, opt => opt.MapFrom(src => src.Repair))
                .ForMember(dest => dest.Technicians, opt => opt.MapFrom(src =>
                    src.JobTechnicians.Select(jt => jt.Technician)));

            // Map VehicleBrand -> VehicleBrandDto
            CreateMap<VehicleBrand, VehicleBrandDto>()
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.BrandID))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.BrandName))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country));

            // Map VehicleModel -> VehicleModelDto
            CreateMap<VehicleModel, VehicleModelDto>()
                .ForMember(dest => dest.ModelId, opt => opt.MapFrom(src => src.ModelID))
                .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.ModelName))
                .ForMember(dest => dest.ManufacturingYear, opt => opt.MapFrom(src => src.ManufacturingYear));

            // Map VehicleColor -> VehicleColorDto
            CreateMap<VehicleColor, VehicleColorDto>()
                .ForMember(dest => dest.ColorId, opt => opt.MapFrom(src => src.ColorID))
                .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.ColorName))
                .ForMember(dest => dest.HexCode, opt => opt.MapFrom(src => src.HexCode));

            // Map Vehicle -> VehicleDto
            CreateMap<Vehicle, VehicleDto>()
                .ForMember(dest => dest.VehicleId, opt => opt.MapFrom(src => src.VehicleId))
                .ForMember(dest => dest.LicensePlate, opt => opt.MapFrom(src => src.LicensePlate))
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color));

            CreateMap<ApplicationUser, CustomerDto>()
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()));

            // Map RepairOrder -> RepairDetailDto
            CreateMap<RepairOrder, RepairDetailDto>()
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.Vehicle.VIN))
                .ForMember(dest => dest.VehicleLicensePlate, opt => opt.MapFrom(src => src.Vehicle.LicensePlate))
                .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                   $"{src.User.FirstName} {src.User.LastName}".Trim()))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.Jobs, opt => opt.MapFrom(src => src.Jobs));
            //emergency
            CreateMap<CreateEmergencyRequestDto, RequestEmergency>();

        }
    }
}
