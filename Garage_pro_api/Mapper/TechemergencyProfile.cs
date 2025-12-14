using AutoMapper;
using BusinessObject.RequestEmergency;
using Dtos.EmergencyTechnicians;
using Services.EmergencyRequestService;

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

            CreateMap<RequestEmergency, EmergencyDetailDto>()
                        .ForMember(d => d.CustomerName,
                            o => o.MapFrom(s => s.Customer != null ? s.Customer.FullName : null))
                        .ForMember(d => d.CustomerPhone,
                            o => o.MapFrom(s => s.Customer != null ? s.Customer.PhoneNumber : null))

                        .ForMember(d => d.BranchName,
                            o => o.MapFrom(s => s.Branch != null ? s.Branch.BranchName : null))
                        .ForMember(d => d.BranchAddress,
                            o => o.MapFrom(s =>
                                s.Branch == null
                                    ? null
                                    : string.Join(" ",
                                        new[] { s.Branch.Street, s.Branch.Commune, s.Branch.Province }
                                            .Where(x => !string.IsNullOrWhiteSpace(x))
                                      )
                            ))

                        .ForMember(d => d.VehiclePlate,
                            o => o.MapFrom(s => s.Vehicle != null ? s.Vehicle.LicensePlate : null))
                        .ForMember(d => d.VehicleName,
                            o => o.MapFrom(s =>
                                s.Vehicle == null
                                    ? null
                                    : string.Join(" ",
                                        new[]
                                        {
                                            s.Vehicle.Brand != null ? s.Vehicle.Brand.BrandName : null,
                                            s.Vehicle.Model != null ? s.Vehicle.Model.ModelName : null
                                        }.Where(x => !string.IsNullOrWhiteSpace(x))
                                      )
                            ))

                        .ForMember(d => d.TechnicianName,
                            o => o.MapFrom(s => s.Technician != null ? s.Technician.FullName : null))

                        // Enum -> string
                        .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
                        .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))                      ;


                            }
                        }
}
