using AutoMapper;
using BusinessObject.Authentication;
using BusinessObject.InspectionAndRepair;
using BusinessObject;
using Dtos.InspectionAndRepair;
using System.Linq;

namespace Garage_pro_api.Mapper
{
    public class JobTechnicianProfile : Profile
    {
        public JobTechnicianProfile()
        {
            CreateMap<Job, JobTechnicianDto>()
               .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.ServiceName))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
               .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.RepairOrder.Vehicle))
               .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.RepairOrder.Vehicle.User))
               .ForMember(dest => dest.Parts, opt => opt.MapFrom(src =>
                   src.JobParts
                       .GroupBy(jp => new { jp.Part.PartCategoryId, jp.Part.PartCategory.CategoryName })
                       .Select(g => new PartCategoryGroupDto
                       {
                           PartCategoryId = g.Key.PartCategoryId,
                           CategoryName = g.Key.CategoryName,
                           Parts = g.Select(jp => new PartDto
                           {
                               PartId = jp.Part.PartId,
                               PartName = jp.Part.Name
                           }).ToList()
                       }).ToList()
               ))
               .ForMember(dest => dest.Repair, opt => opt.MapFrom(src => src.Repair))
              .ForMember(dest => dest.Technicians, opt => opt.MapFrom(src =>
                   src.JobTechnicians.Select(jt => jt.Technician)))
               .ReverseMap();

            CreateMap<Vehicle, VehicleDto>().ReverseMap();

            CreateMap<ApplicationUser, CustomerDto>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()));

            CreateMap<Repair, RepairDto>().ReverseMap();

            CreateMap<Technician, TechnicianDto>()
               .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                   $"{src.User.FirstName} {src.User.LastName}".Trim()))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
               .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber));
        }
    }
}