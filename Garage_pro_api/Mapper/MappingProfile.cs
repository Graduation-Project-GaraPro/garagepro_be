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
            CreateMap<Vehicle, VehicleDto>().ReverseMap();
            CreateMap<CreateVehicleDto, Vehicle>();
            CreateMap<UpdateVehicleDto, Vehicle>();
            
            // User mappings
            CreateMap<ApplicationUser, CustomerDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => ""));

            // Quotation mappings
            CreateMap<Quotation, QuotationDto>().ReverseMap();
            CreateMap<QuotationService, QuotationServiceDto>().ReverseMap();
            CreateMap<QuotationServicePart, QuotationServicePartDto>().ReverseMap();
        }
    }
}