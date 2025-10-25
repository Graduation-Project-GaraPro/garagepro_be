using BusinessObject;
using Dtos.InspectionAndRepair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.InspectionAndRepair
{
    public interface IInspectionTechnicianService
    {
        Task<List<InspectionTechnicianDto>> GetInspectionsByTechnicianAsync(string userId);
        Task<InspectionTechnicianDto> GetInspectionByIdAsync(Guid id, string userId);
        Task<InspectionTechnicianDto> UpdateInspectionAsync(Guid id, UpdateInspectionRequest request, string userId);
        Task<InspectionTechnicianDto> StartInspectionAsync(Guid id, string userId);
        Task<bool> RemovePartFromInspectionAsync(Guid inspectionId, Guid serviceId, Guid partId, string userId);

    }
}
