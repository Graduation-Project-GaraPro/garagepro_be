namespace Garage_pro_api.Mapper
{
    using AutoMapper;
    using BusinessObject.InspectionAndRepair;
    using BusinessObject;
    using Dtos.RepairOrderArchivedDtos;

    public class RepairOrderArchivedProfile : Profile
    {
        public RepairOrderArchivedProfile()
        {
            // List item
            CreateMap<RepairOrder, RepairOrderArchivedListItemDto>()
                .ForMember(d => d.BranchName,
                    opt => opt.MapFrom(s => s.Branch.BranchName))
                .ForMember(d => d.LicensePlate,
                    opt => opt.MapFrom(s => s.Vehicle.LicensePlate))
                .ForMember(d => d.ModelName,
                    opt => opt.MapFrom(s => s.Vehicle.Model.ModelName))
                .ForMember(d => d.BrandName,
                    opt => opt.MapFrom(s => s.Vehicle.Brand.BrandName));

            // Detail
            CreateMap<RepairOrder, RepairOrderArchivedDetailDto>()
                .ForMember(d => d.BranchName,
                    opt => opt.MapFrom(s => s.Branch.BranchName))
                .ForMember(d => d.LicensePlate,
                    opt => opt.MapFrom(s => s.Vehicle.LicensePlate))
                .ForMember(d => d.ModelName,
                    opt => opt.MapFrom(s => s.Vehicle.Model.ModelName))
                 .ForMember(d => d.BrandName,
                    opt => opt.MapFrom(s => s.Vehicle.Brand.BrandName))
                .ForMember(d => d.Jobs,
                    opt => opt.MapFrom(s => s.Jobs));

            // Job
            CreateMap<Job, RepairOrderArchivedJobDto>()
                .ForMember(d => d.Repair,
                    opt => opt.MapFrom(s => s.Repair))
                .ForMember(d => d.Technicians,
                    opt => opt.MapFrom(s => s.JobTechnicians))
                .ForMember(d => d.Parts,
                    opt => opt.MapFrom(s => s.JobParts));

            // Repair
            CreateMap<Repair, RepairOrderArchivedRepairDto>();

            // Technician mapping qua JobTechnician
            CreateMap<JobTechnician, RepairOrderArchivedTechnicianDto>()
                .ForMember(d => d.TechnicianId,
                    opt => opt.MapFrom(s => s.TechnicianId))
                .ForMember(d => d.FullName,
                    opt => opt.MapFrom(s => s.Technician.User.FirstName + s.Technician.User.LastName)); // đổi property nếu khác

            // JobPart -> DTO
            CreateMap<JobPart, RepairOrderArchivedJobPartDto>()
                .ForMember(d => d.PartName,
                    opt => opt.MapFrom(s => s.Part.Name));
        }
    }

}
