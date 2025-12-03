using BusinessObject.InspectionAndRepair;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.InspectionAndRepair
{
    public interface ITechnicianRepository
    {
        Task<Technician?> GetByIdAsync(Guid technicianId);
        Task<List<Technician>> GetAllAsync();
        Task<Technician?> GetByUserIdAsync(string userId);
        Task<Technician> CreateAsync(Technician technician);
        Task<Technician> UpdateAsync(Technician technician);
        Task<bool> DeleteAsync(Guid technicianId);
    }
}
