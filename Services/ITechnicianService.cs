using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dtos.Job;

namespace Services
{
    public interface ITechnicianService
    {
        /// Get all technicians with their schedule information
        Task<IEnumerable<TechnicianWorkloadDto>> GetAllTechnicianWorkloadsAsync(TechnicianScheduleFilterDto? filter = null);

        /// Get schedule information for a specific technician
        Task<TechnicianWorkloadDto?> GetTechnicianWorkloadAsync(Guid technicianId);

        /// Get detailed schedule for all technicians
        Task<IEnumerable<TechnicianScheduleDto>> GetAllTechnicianSchedulesAsync(TechnicianScheduleFilterDto? filter = null);

        /// Get detailed schedule for a specific technician
        Task<IEnumerable<TechnicianScheduleDto>> GetTechnicianScheduleAsync(Guid technicianId, TechnicianScheduleFilterDto? filter = null);
    }
}