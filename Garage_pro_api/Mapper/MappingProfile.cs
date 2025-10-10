using AutoMapper;
using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Branches;
using BusinessObject.Customers;
using BusinessObject.Policies;
using BusinessObject.Roles;
using Customers;
using Dtos.Branches;
using Dtos.Customers;
using Dtos.Policies;
using Dtos.Roles;
using Dtos.Services;
using Dtos.Parts;
using Dtos.Vehicles;

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
                           opt => opt.MapFrom(src => src.BranchServices.Select(bs => bs.Branch)))
                .ForMember(dest => dest.Parts,
                           opt => opt.MapFrom(src => src.ServiceParts.Select(bs => bs.Part)));


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
            CreateMap<Part, PartServiceRelatedDto>().ReverseMap();

            // PartCategory -> PartCategoryWithPartsDto
            CreateMap<PartCategory, PartCategoryWithPartsDto>()
                .ForMember(dest => dest.PartCategoryId, opt => opt.MapFrom(src => src.LaborCategoryId));

            //mapping cho Vehicle
            CreateMap<Vehicle, VehicleDto>();
            CreateMap<RequestPartDto, RequestPart>()
    .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => Guid.NewGuid()));

            CreateMap<RequestServiceDto, RequestService>()
                .ForMember(dest => dest.RequestServiceId, opt => opt.MapFrom(src => Guid.NewGuid()));
            //quotation
            CreateMap<Inspection, QuotationDto>()
            .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.ServiceInspections))
            .ForMember(dest => dest.Parts, opt => opt.MapFrom(src => src.PartInspections));

            CreateMap<ServiceInspection, ServiceItemDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.ServiceName))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Service.Price))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => 1));

            CreateMap<PartInspection, PartItemDto>()
                .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => src.PartId))
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.Part.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Part.Price))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => 1))
                .ForMember(dest => dest.SelectedSpecId, opt => opt.MapFrom(src => src.PartInspectionId))
                .ForMember(dest => dest.Specifications, opt => opt.MapFrom(src => src.Part.PartSpecifications));

            //Map repair request
            CreateMap<RepairRequest, RepairRequestDto>()
               .ForMember(dest => dest.RepairRequestID, opt => opt.MapFrom(src => src.RepairRequestID))
               .ForMember(dest => dest.VehicleID, opt => opt.MapFrom(src => src.VehicleID))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
               .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src =>
                   src.RepairImages.Select(img => img.ImageUrl).ToList()))
               .ForMember(dest => dest.RequestServices, opt => opt.MapFrom(src => src.RequestServices));
            // Map reuqest Servcie
            CreateMap<RequestService, RequestServiceDto>()
     .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.ServiceId))
     .ForMember(dest => dest.Parts, opt => opt.MapFrom(src => src.RequestParts));

            CreateMap<RequestPart, RequestPartDto>()
                .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => src.PartId));
                


            CreateMap<PartSpecification, PartSpecificationDto>();
        }
    }
}
