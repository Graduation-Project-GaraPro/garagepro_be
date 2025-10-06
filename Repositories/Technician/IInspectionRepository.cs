using BusinessObject;
using Dtos.Technician;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Technician
{
    public interface IInspectionRepository
    {
        Task<List<Inspection>> GetInspectionsByTechnicianAsync(string userId);
        Task<Inspection> GetInspectionByIdAsync(Guid id, string userId);
        Task<Inspection> UpdateInspectionAsync(Guid id, UpdateInspectionRequest request, string userId);
        Task<Inspection> StartInspectionAsync(Guid id, string userId);
    }   
}
