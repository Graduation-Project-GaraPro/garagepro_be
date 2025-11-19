using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using Dtos.Bills;

namespace Garage_pro_api.Mapper
{
    public class RepairOrderBillProfile : Profile
    {
        public RepairOrderBillProfile()
        {
            // RepairOrder -> RepairOrderPaymentDto
            CreateMap<RepairOrder, RepairOrderPaymentDto>()
                .ForMember(dest => dest.ApprovedQuotations,
                    opt => opt.MapFrom(src => src.Quotations
                        .Where(q => q.Status == QuotationStatus.Approved)));

            // Vehicle -> VehicleDto
            CreateMap<Vehicle, VehicleDto>()
                .ForMember(dest => dest.BrandName,
                    opt => opt.MapFrom(src => src.Brand.BrandName))
                .ForMember(dest => dest.ModelName,
                    opt => opt.MapFrom(src => src.Model.ModelName));

            // QuotationServicePart -> QuotationServicePartDto
            CreateMap<QuotationServicePart, QuotationServicePartDto>()
                .ForMember(dest => dest.PartName,
                    opt => opt.MapFrom(src => src.Part.Name));

            // QuotationService -> QuotationServiceDto
            CreateMap<QuotationService, QuotationServiceDto>()
                .ForMember(dest => dest.ServiceName,
                    opt => opt.MapFrom(src => src.Service != null ? src.Service.ServiceName : null))
                .ForMember(dest => dest.Parts,
                    opt => opt.MapFrom(src => src.QuotationServiceParts));

            // Quotation -> ApprovedQuotationDto
            CreateMap<Quotation, ApprovedQuotationDto>()
                .ForMember(dest => dest.Services,
                    opt => opt.MapFrom(src => src.QuotationServices));
        }
    }
}
