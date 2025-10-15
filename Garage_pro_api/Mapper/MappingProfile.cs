using AutoMapper;
using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Branches;
using BusinessObject.Customers;
using BusinessObject.Policies;
using BusinessObject.Roles;
using Dtos.Branches;
using Dtos.Customers;
using Dtos.Policies;
using Dtos.Roles;
using Dtos.Vehicles;
using Dtos.Services;
using Dtos.Parts;
using Dtos.Auth;
using Dtos.Quotations;

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

            // Vehicle mappings
            CreateMap<Vehicle, VehicleDto>()
                .ForMember(dest => dest.VehicleID, opt => opt.MapFrom(src => src.VehicleId))
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.BrandID, opt => opt.MapFrom(src => src.BrandId))
                .ForMember(dest => dest.ModelID, opt => opt.MapFrom(src => src.ModelId))
                .ForMember(dest => dest.ColorID, opt => opt.MapFrom(src => src.ColorId))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
                .ForMember(dest => dest.Odometer, opt => opt.MapFrom(src => src.Odometer))
                .ForMember(dest => dest.LastServiceDate, opt => opt.MapFrom(src => src.LastServiceDate))
                .ForMember(dest => dest.NextServiceDate, opt => opt.MapFrom(src => src.NextServiceDate))
                .ForMember(dest => dest.WarrantyStatus, opt => opt.MapFrom(src => src.WarrantyStatus))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.BrandName))
                .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.Model.ModelName))
                .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.Color.ColorName));

            CreateMap<CreateVehicleDto, Vehicle>()
                .ForMember(dest => dest.VehicleId, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserID))
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.BrandID))
                .ForMember(dest => dest.ModelId, opt => opt.MapFrom(src => src.ModelID))
                .ForMember(dest => dest.ColorId, opt => opt.MapFrom(src => src.ColorID))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
                .ForMember(dest => dest.LicensePlate, opt => opt.MapFrom(src => src.LicensePlate.ToUpper()))
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN))
                .ForMember(dest => dest.Odometer, opt => opt.MapFrom(src => src.Odometer))
                .ForMember(dest => dest.LastServiceDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.NextServiceDate, opt => opt.Ignore())
                .ForMember(dest => dest.WarrantyStatus, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Model, opt => opt.Ignore())
                .ForMember(dest => dest.Color, opt => opt.Ignore())
                .ForMember(dest => dest.RepairOrders, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<UpdateVehicleDto, Vehicle>()
                .ForMember(dest => dest.VehicleId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.BrandID))
                .ForMember(dest => dest.ModelId, opt => opt.MapFrom(src => src.ModelID))
                .ForMember(dest => dest.ColorId, opt => opt.MapFrom(src => src.ColorID))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
                .ForMember(dest => dest.LicensePlate, opt => opt.MapFrom(src => src.LicensePlate.ToUpper()))
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN))
                .ForMember(dest => dest.Odometer, opt => opt.MapFrom(src => src.Odometer))
                .ForMember(dest => dest.NextServiceDate, opt => opt.MapFrom(src => src.NextServiceDate))
                .ForMember(dest => dest.WarrantyStatus, opt => opt.MapFrom(src => src.WarrantyStatus))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Model, opt => opt.Ignore())
                .ForMember(dest => dest.Color, opt => opt.Ignore())
                .ForMember(dest => dest.RepairOrders, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // Quotation mappings
            CreateMap<Quotation, QuotationDto>()
                .ForMember(dest => dest.QuotationId, opt => opt.MapFrom(src => src.QuotationId))
                .ForMember(dest => dest.InspectionId, opt => opt.MapFrom(src => src.InspectionId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.VehicleId, opt => opt.MapFrom(src => src.VehicleId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.SentToCustomerAt, opt => opt.MapFrom(src => src.SentToCustomerAt))
                .ForMember(dest => dest.CustomerResponseAt, opt => opt.MapFrom(src => src.CustomerResponseAt))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.DiscountAmount))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.ExpiresAt, opt => opt.MapFrom(src => src.ExpiresAt))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.VehicleInfo, opt => opt.MapFrom(src => $"{src.Vehicle.Brand.BrandName} {src.Vehicle.Model.ModelName}"))
                .ForMember(dest => dest.QuotationServiceParts, opt => opt.Ignore());

            CreateMap<QuotationService, QuotationServiceDto>()
                .ForMember(dest => dest.QuotationServiceId, opt => opt.MapFrom(src => src.QuotationServiceId))
                .ForMember(dest => dest.QuotationId, opt => opt.MapFrom(src => src.QuotationId))
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.ServiceId))
                .ForMember(dest => dest.IsSelected, opt => opt.MapFrom(src => src.IsSelected))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.ServiceName))
                .ForMember(dest => dest.ServiceDescription, opt => opt.MapFrom(src => src.Service.Description))
                .ForMember(dest => dest.QuotationServiceParts, opt => opt.MapFrom(src => src.QuotationServiceParts));


            // Add mapping for QuotationServicePart
            CreateMap<QuotationServicePart, QuotationServicePartDto>()
                .ForMember(dest => dest.QuotationServicePartId, opt => opt.MapFrom(src => src.QuotationServicePartId))
                .ForMember(dest => dest.QuotationServiceId, opt => opt.MapFrom(src => src.QuotationServiceId))
                .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => src.PartId))
                .ForMember(dest => dest.IsSelected, opt => opt.MapFrom(src => src.IsSelected))
                .ForMember(dest => dest.IsRecommended, opt => opt.MapFrom(src => src.IsRecommended))
                .ForMember(dest => dest.RecommendationNote, opt => opt.MapFrom(src => src.RecommendationNote))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.Part.Name))
                .ForMember(dest => dest.PartDescription, opt => opt.MapFrom(src => src.Part.Name));

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
            CreateMap<RequestPartDto, RequestPart>()
                .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => Guid.NewGuid()));

            CreateMap<RequestServiceDto, RequestService>()
                .ForMember(dest => dest.RequestServiceId, opt => opt.MapFrom(src => Guid.NewGuid()));

            //CreateMap<ServiceInspection, ServiceItemDto>()
            //    .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.ServiceName))
            //    .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Service.Price))
            //    .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => 1));

            //CreateMap<PartInspection, PartItemDto>()
            //    .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => src.PartId))
            //    .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.Part.Name))
            //    .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Part.Price))
            //    .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => 1))
            //    .ForMember(dest => dest.SelectedSpecId, opt => opt.MapFrom(src => src.PartInspectionId))
            //    .ForMember(dest => dest.Specifications, opt => opt.MapFrom(src => src.Part.PartSpecifications));

            //CreateMap<PartSpecification, PartSpecificationDto>();
        }
    }
}