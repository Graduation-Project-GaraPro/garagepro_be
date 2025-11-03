using AutoMapper;
using BusinessObject.InspectionAndRepair;
using Dtos.Statistical;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Garage_pro_api.Mapper
{
    public class StatisticalProfile : Profile
    {
        public StatisticalProfile() 
        {
            CreateMap<Technician, TechnicianStatisticDto>();
        }
    }
}
