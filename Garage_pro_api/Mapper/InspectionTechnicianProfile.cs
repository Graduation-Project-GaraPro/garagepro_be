using AutoMapper;
using BusinessObject;
using BusinessObject.Vehicles;
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

            CreateMap<RepairOrder, RepairOrderDto>()
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Vehicle.User))
                .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.RepairOrderServices));
            // Map Vehicle with nested Brand, Model, Color
            CreateMap<Vehicle, VehicleDto>()
                .ForMember(d => d.Brand, o => o.MapFrom(s => s.Brand))
                .ForMember(d => d.Model, o => o.MapFrom(s => s.Model))
                .ForMember(d => d.Color, o => o.MapFrom(s => s.Color));

            // Map Brand, Model, Color separately
            CreateMap<VehicleBrand, VehicleBrandDto>()
                .ForMember(d => d.BrandId, o => o.MapFrom(s => s.BrandID));

            CreateMap<VehicleModel, VehicleModelDto>()
                .ForMember(d => d.ModelId, o => o.MapFrom(s => s.ModelID));

            CreateMap<VehicleColor, VehicleColorDto>()
                .ForMember(d => d.ColorId, o => o.MapFrom(s => s.ColorID));

            CreateMap<BusinessObject.Authentication.ApplicationUser, CustomerDto>()
                .ForMember(d => d.CustomerId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}".Trim()));

             

            CreateMap<RepairOrderService, RepairOrderServiceDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.ServiceName))
                 .ForMember(dest => dest.Parts, opt => opt.MapFrom(src => src.RepairOrderServiceParts))
                  .ForMember(dest => dest.IsAdvanced, opt => opt.MapFrom(src => src.Service.IsAdvanced))
              .ForMember(dest => dest.AllServiceParts, opt => opt.MapFrom(src => src.Service.ServiceParts));

            CreateMap<RepairOrderServicePart, RepairOrderServicePartDto>()
              .ForMember(d => d.PartName, o => o.MapFrom(s => s.Part.Name));

            CreateMap<BusinessObject.Branches.ServicePart, ServicePartDto>()
              .ForMember(d => d.PartName, o => o.MapFrom(s => s.Part.Name));

            CreateMap<ServiceInspection, ServiceInspectionDto>()
                .ForMember(d => d.ServiceName, o => o.MapFrom(s => s.Service.ServiceName))
                .ForMember(d => d.ConditionStatus, o => o.MapFrom(s => (int)s.ConditionStatus));

            CreateMap<PartInspection, PartInspectionDto>();
                
        }
    }
}
