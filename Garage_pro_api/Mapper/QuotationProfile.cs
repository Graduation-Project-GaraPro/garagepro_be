using AutoMapper;
using BusinessObject;
using Dtos.Quotations;

namespace Garage_pro_api.Mapper
{
    public class QuotationProfile : Profile
    {
        public QuotationProfile()
        {
            // Quotation → QuotationDetailDto
            CreateMap<Quotation, QuotationDetailDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.VehicleInfo, opt => opt.MapFrom(src => src.Vehicle != null
                    ? $"{src.Vehicle.LicensePlate} - {src.Vehicle.Model.ModelName}"
                    : string.Empty))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.QuotationServices, opt => opt.MapFrom(src => src.QuotationServices))
                .ForMember(dest => dest.Inspection, opt => opt.MapFrom(src => src.Inspection))
                .ForMember(dest => dest.IsArchived, opt => opt.MapFrom(src => src.RepairOrder.IsArchived))
                .ForMember(dest => dest.ArchivedAt, opt => opt.MapFrom(src => src.RepairOrder.ArchivedAt));

            // QuotationService → QuotationServiceDetailDto
            CreateMap<QuotationService, QuotationServiceDetailDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.ServiceName))
                .ForMember(dest => dest.ServiceDescription, opt => opt.MapFrom(src => src.Service.Description))
                .ForMember(dest => dest.PartCategories, opt => opt.Ignore()) // sẽ gán thủ công sau
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src =>
                    src.QuotationServiceParts.Sum(p => p.Quantity))) // optional
                .ForMember(dest => dest.IsAdvanced, otp => otp.MapFrom(s => s.Service.IsAdvanced));
                

            // QuotationServicePart → QuotationPart
            CreateMap<QuotationServicePart, QuotationPart>()
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.Part.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));

            // Inspection → InspectionDto
            CreateMap<Inspection, InspectionDto>();

        }
    }
}
