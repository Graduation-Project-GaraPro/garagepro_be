using AutoMapper;
using BusinessObject;
using BusinessObject.Customers;
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
                .ForMember(d => d.ServiceInspections, o => o.MapFrom(s => s.ServiceInspections));

            CreateMap<RepairOrder, RepairOrderDto>()
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Vehicle.User))
                .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.RepairOrderServices))
                .ForMember(dest => dest.RepairImages, opt => opt.MapFrom(src =>
                    src.RepairRequest != null ? src.RepairRequest.RepairImages : new List<RepairImage>()));

            CreateMap<Vehicle, VehicleDto>()
                .ForMember(d => d.Brand, o => o.MapFrom(s => s.Brand))
                .ForMember(d => d.Model, o => o.MapFrom(s => s.Model));

            CreateMap<VehicleBrand, VehicleBrandDto>()
                .ForMember(d => d.BrandId, o => o.MapFrom(s => s.BrandID));

            CreateMap<VehicleModel, VehicleModelDto>()
                .ForMember(d => d.ModelId, o => o.MapFrom(s => s.ModelID));
        
            CreateMap<BusinessObject.Authentication.ApplicationUser, CustomerDto>()
                .ForMember(d => d.CustomerId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}".Trim()));

            CreateMap<RepairOrderService, RepairOrderServiceDto>()
             .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.ServiceName))
             .ForMember(dest => dest.ServicePrice, opt => opt.MapFrom(src => src.Service.Price))
             .ForMember(dest => dest.IsAdvanced, opt => opt.MapFrom(src => src.Service.IsAdvanced))
             .ForMember(dest => dest.AllPartCategories, opt => opt.MapFrom(src =>
                 src.Service.ServicePartCategories.Select(spc => spc.PartCategory).ToList()))
             .ForMember(dest => dest.ServiceCategoryId, opt => opt.MapFrom(src => src.Service.ServiceCategoryId))
             .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Service.ServiceCategory != null ? src.Service.ServiceCategory.CategoryName : null));

            //CreateMap<BusinessObject.Branches.ServicePart, ServicePartDto>()
            //  .ForMember(d => d.PartName, o => o.MapFrom(s => s.Part.Name))
            //  .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Part.Price));

            CreateMap<ServiceInspection, ServiceInspectionDto>()
                .ForMember(d => d.ServiceName, o => o.MapFrom(s => s.Service.ServiceName))
                .ForMember(d => d.ConditionStatus, o => o.MapFrom(s => (int)s.ConditionStatus))
                .ForMember(d => d.ServiceCategoryId, o => o.MapFrom(s => s.Service.ServiceCategoryId))
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Service.ServiceCategory != null ? s.Service.ServiceCategory.CategoryName : null))
                .ForMember(d => d.IsAdvanced, o => o.MapFrom(s => s.Service.IsAdvanced))
                .ForMember(d => d.AllPartCategories, o => o.MapFrom(s =>
                    s.Service.ServicePartCategories.Select(spc => spc.PartCategory).ToList()));


            CreateMap<Service, AllServiceDto>()
                .ForMember(d => d.ServiceId, o => o.MapFrom(s => s.ServiceId))
                .ForMember(d => d.ServiceCategoryId, o => o.MapFrom(s => s.ServiceCategoryId))
                .ForMember(d => d.ServiceName, o => o.MapFrom(s => s.ServiceName))
                .ForMember(d => d.Price, o => o.MapFrom(s => s.Price))
                .ForMember(d => d.IsAdvanced, o => o.MapFrom(s => s.IsAdvanced))
                .ForMember(d => d.ServiceCategoryId, o => o.MapFrom(s => s.ServiceCategoryId))
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.ServiceCategory != null ? s.ServiceCategory.CategoryName : null));

            CreateMap<PartCategory, PartCategoryDto>()
                .ForMember(d => d.PartCategoryId, o => o.MapFrom(s => s.LaborCategoryId))
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.CategoryName))
                .ForMember(d => d.Parts, o => o.MapFrom(s => s.Parts));

            CreateMap<Part, ServicePartDto>()
                .ForMember(d => d.ServicePartId, o => o.MapFrom(s => s.PartId))
                .ForMember(d => d.PartId, o => o.MapFrom(s => s.PartId))
                .ForMember(d => d.PartName, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.Price));

            CreateMap<PartInspection, PartInspectionDto>()
                .ForMember(d => d.PartName, o => o.MapFrom(s => s.Part.Name))
                .ForMember(d => d.PartCategoryId, o => o.MapFrom(s => s.PartCategoryId))
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.PartCategory.CategoryName))
                .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Quantity));

            CreateMap<RepairImage, RepairImageDto>()
               .ForMember(d => d.ImageId, o => o.MapFrom(s => s.ImageId))
               .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.ImageUrl));
        }
    }
}
