using AutoMapper;
using BusinessObject.InspectionAndRepair;
using BusinessObject;
using Dtos.InspectionAndRepair;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
                .ForMember(dest => dest.Repairs, opt => opt.MapFrom(src => src.Repair));

            CreateMap<RepairOrder, RepairDetailDto>()
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.Vehicle.VIN))
                .ForMember(dest => dest.VehicleLicensePlate, opt => opt.MapFrom(src => src.Vehicle.LicensePlate))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.Jobs, opt => opt.MapFrom(src => src.Jobs));
         
        }
    }
}
