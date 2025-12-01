using AutoMapper;
using BusinessObject.RequestEmergency;
using Dtos.EmergencyTechnicians;

namespace Garage_pro_api.Mapper
{
    public class TechemergencyProfile : Profile
    {
        public TechemergencyProfile()
        {
            CreateMap<RequestEmergency, EmergencyForTechnicianDto>()
                .ForMember(dest => dest.CustomerName,
                           opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : "Unknown"))
                .ForMember(dest => dest.PhoneNumber,
                           opt => opt.MapFrom(src => src.Customer != null ? src.Customer.PhoneNumber : "Unknown"))
                .ForMember(dest => dest.BranchLatitude,
                               opt => opt.MapFrom(src => src.Branch.Latitude))
                .ForMember(dest => dest.BranchLongitude,
                               opt => opt.MapFrom(src => src.Branch.Longitude))
                .ForMember(dest => dest.BranchName,
                               opt => opt.MapFrom(src => src.Branch.BranchName))
                ;
        }
    }
}
