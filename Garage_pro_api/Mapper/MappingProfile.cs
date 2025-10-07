using AutoMapper;
using BusinessObject.Branches;
using BusinessObject;
using BusinessObject.Policies;
using BusinessObject.Roles;
using Dtos.Branches;
using Dtos.Policies;
using Dtos.Roles;
using BusinessObject.Authentication;
using Dtos.Services;
using Dtos.Parts;
using Dtos.Auth;

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
            CreateMap<ApplicationUser, ApplicationUserDto>().ReverseMap();
            CreateMap<ApplicationRole, RoleDto>()
                .ForMember(dest => dest.PermissionCategories,
                    opt => opt.Ignore()); // mình sẽ gán thủ công sau

            CreateMap<ApplicationUser, UserDto>().ReverseMap();
            CreateMap<UpdateUserDto, ApplicationUser>()
    .ForAllMembers(opt =>
        opt.Condition((src, dest, srcMember) => srcMember != null));


            // Service -> ServiceDto

            CreateMap<Service, Dtos.Branches.ServiceDto>();
            CreateMap<Service, Dtos.Services.ServiceDto>();
            CreateMap<Service, CreateServiceDto>().ReverseMap();
            CreateMap<Service, UpdateServiceDto>().ReverseMap().ForMember(dest => dest.ServiceId, opt => opt.Ignore()); ;

            CreateMap<Service, Dtos.Services.ServiceDto>()
                .ForMember(dest => dest.Branches,
                           opt => opt.MapFrom(src => src.BranchServices.Select(bs => bs.Branch)));


            CreateMap<ServiceCategory, ServiceCategoryDto>().ReverseMap();
            CreateMap<ServiceCategory, GetCategoryForServiceDto>().ReverseMap();

            // DTO -> Entity (Create)
            CreateMap<CreateServiceCategoryDto, ServiceCategory>();

            // DTO -> Entity (Update)
            CreateMap<UpdateServiceCategoryDto, ServiceCategory>();


            // Branch -> BranchReadDto
            CreateMap<Branch, BranchReadDto>()
                .ForMember(dest => dest.Services,
                           opt => opt.MapFrom(src => src.BranchServices.Select(bs => bs.Service)))
                .ForMember(dest => dest.OperatingHours,
                           opt => opt.MapFrom(src => src.OperatingHours)).ReverseMap();

            CreateMap<Branch, BranchServiceRelatedDto>().ReverseMap();


            CreateMap<Branch, BranchCreateDto>()
               .ReverseMap();

            CreateMap<Branch, BranchUpdateDto>()
           .ReverseMap();
            // OperatingHour -> OperatingHourDto
            CreateMap<OperatingHour, OperatingHourDto>();

            // Part -> PartDto
            CreateMap<Part, PartDto>();

            // PartCategory -> PartCategoryWithPartsDto
            CreateMap<PartCategory, PartCategoryWithPartsDto>()
                .ForMember(dest => dest.PartCategoryId, opt => opt.MapFrom(src => src.LaborCategoryId));
        }
    }
}
