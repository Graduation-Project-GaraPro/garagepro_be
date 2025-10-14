using AutoMapper;
using BusinessObject;
using Dtos.InspectionAndRepair;

namespace Garage_pro_api.Mapper
{
    public class InspectionTechnicianProfile : Profile
    {
        public InspectionTechnicianProfile()
        {
            CreateMap<Inspection, InspectionTechnicianDto>()
                .ForMember(d => d.StatusText, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => (int)s.Status))
                .ForMember(d => d.ServiceInspections, o => o.MapFrom(s => s.ServiceInspections));
                //.ForMember(d => d.PartInspections, o => o.MapFrom(s => s.PartInspections));

            CreateMap<RepairOrder, RepairOrderDto>()
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Vehicle.User))
                .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.RepairOrderServices));
            CreateMap<Vehicle, VehicleDto>();
            CreateMap<BusinessObject.Authentication.ApplicationUser, CustomerDto>()
                .ForMember(d => d.CustomerId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}".Trim()));

            CreateMap<RepairOrderService, RepairOrderServiceDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.ServiceName));

            CreateMap<ServiceInspection, ServiceInspectionDto>()
                .ForMember(d => d.ServiceName, o => o.MapFrom(s => s.Service.ServiceName))
                .ForMember(d => d.ConditionStatus, o => o.MapFrom(s => (int)s.ConditionStatus));

            CreateMap<PartInspection, PartInspectionDto>();
                
        }
    }
}
