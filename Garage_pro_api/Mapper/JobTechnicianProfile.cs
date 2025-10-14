using AutoMapper;
using BusinessObject.Authentication;
using BusinessObject.InspectionAndRepair;
using BusinessObject;
using Dtos.InspectionAndRepair;

namespace Garage_pro_api.Mapper
{
    public class JobTechnicianProfile : Profile
    {
        public JobTechnicianProfile()
        {
            CreateMap<Job, JobTechnicianDto>()
               .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.ServiceName))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
               .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.RepairOrder.Vehicle))
               .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.RepairOrder.Vehicle.User))
               .ForMember(dest => dest.Parts, opt => opt.MapFrom(src => src.JobParts.Select(jp => jp.Part)))
               .ForMember(dest => dest.Repair, opt => opt.MapFrom(src => src.Repair))
               .ReverseMap();

            CreateMap<Vehicle, VehicleDto>().ReverseMap();
            CreateMap<ApplicationUser, CustomerDto>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()));
            CreateMap<Part, PartDto>()
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.Name));
            CreateMap<Repair, RepairDto>().ReverseMap();
        }
    }
}
