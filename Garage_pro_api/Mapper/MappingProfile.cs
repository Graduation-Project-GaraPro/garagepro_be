using AutoMapper;
using BusinessObject.Policies;
using BusinessObject.Roles;
using Dtos.Policies;
using Dtos.Roles;
using BusinessObject;
using Dtos.Vehicle;
using BusinessObject.Authentication;
using Dtos.Quotation; // Add this line

namespace Garage_pro_api.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // SecurityPolicy ↔ SecurityPolicyDto
            CreateMap<SecurityPolicy, SecurityPolicyDto>().ReverseMap();

            // AuditHistory ↔ AuditHistoryDto
            CreateMap<SecurityPolicyHistory, AuditHistoryDto>()
                .ForMember(dest => dest.HistoryId, opt => opt.MapFrom(src => src.HistoryId))
                .ForMember(dest => dest.PolicyId, opt => opt.MapFrom(src => src.PolicyId))
                .ForMember(dest => dest.ChangedBy, opt => opt.MapFrom(src => src.ChangedBy))
                .ForMember(dest => dest.ChangedAt, opt => opt.MapFrom(src => src.ChangedAt))
                .ForMember(dest => dest.ChangeSummary, opt => opt.MapFrom(src => src.ChangeSummary))
                .ForMember(dest => dest.PreviousValues, opt => opt.MapFrom(src => src.PreviousValues))
                .ForMember(dest => dest.NewValues, opt => opt.MapFrom(src => src.NewValues));

            // Map khi revert snapshot: chỉ copy giá trị cần update
            CreateMap<SecurityPolicy, SecurityPolicy>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // không được ghi đè Id
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // giữ nguyên
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // sẽ set thủ công


            // Permission → PermissionDto
            CreateMap<Permission, PermissionDto>();

            // Category → CategoryDto (gồm list PermissionDto)
            CreateMap<PermissionCategory, PermissionCategoryDto>()
                .ForMember(dest => dest.Permissions,
                    opt => opt.MapFrom(src => src.Permissions));

            // Role → RoleDto
            CreateMap<ApplicationRole, RoleDto>()
                .ForMember(dest => dest.PermissionCategories,
                    opt => opt.Ignore()); // mình sẽ gán thủ công sau

            // Vehicle mappings
            CreateMap<Vehicle, VehicleDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.VehicleId))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Brand, opt => opt.Ignore()) // Would need lookup from Brand entity
                .ForMember(dest => dest.Model, opt => opt.Ignore()) // Would need lookup from Model entity
                .ForMember(dest => dest.Color, opt => opt.Ignore()) // Would need lookup from Color entity
                .ReverseMap();
                
            CreateMap<CreateVehicleDto, Vehicle>()
                .ForMember(dest => dest.VehicleId, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.LicensePlate, opt => opt.MapFrom(src => src.LicensePlate.ToUpper()))
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => Guid.NewGuid())) // Temporary
                .ForMember(dest => dest.ModelId, opt => opt.MapFrom(src => Guid.NewGuid())) // Temporary
                .ForMember(dest => dest.ColorId, opt => opt.MapFrom(src => Guid.NewGuid())) // Temporary
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN))
                .ForMember(dest => dest.Odometer, opt => opt.MapFrom(src => src.Odometer))
                .ForMember(dest => dest.LastServiceDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateVehicleDto, Vehicle>()
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN))
                .ForMember(dest => dest.Odometer, opt => opt.MapFrom(src => src.Odometer))
                .ForMember(dest => dest.NextServiceDate, opt => opt.MapFrom(src => src.NextServiceDate))
                .ForMember(dest => dest.WarrantyStatus, opt => opt.MapFrom(src => src.WarrantyStatus));

            // Quotation mappings
            CreateMap<Quotation, QuotationDto>().ReverseMap();
            CreateMap<QuotationService, QuotationServiceDto>().ReverseMap();
            CreateMap<QuotationServicePart, QuotationServicePartDto>().ReverseMap();
        }
    }
}