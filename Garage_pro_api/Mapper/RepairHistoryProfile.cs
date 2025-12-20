using AutoMapper;
using BusinessObject;
using Dtos.RepairHistory;

namespace Garage_pro_api.Mapper
{
    public class RepairHistoryProfile : Profile
    {
        public RepairHistoryProfile()
        {
            CreateMap<Job, JobHistoryDto>();

            CreateMap<JobPart, JobPartDto>()
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.Part.Name))
                .ForMember(dest => dest.WarrantyMonths, opt => opt.MapFrom(src => src.WarrantyMonths))
                .ForMember(dest => dest.WarrantyStartAt, opt => opt.MapFrom(src => src.WarrantyStartAt))
                .ForMember(dest => dest.WarrantyEndAt, opt => opt.MapFrom(src => src.WarrantyEndAt)); 
            CreateMap<RepairOrderService, ServiceDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.ServiceName));
        }
    }
}
