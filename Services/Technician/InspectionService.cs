using BusinessObject;
using Dtos.Technician;
using Repositories.Technician;

namespace Services.Technician
{
    public class InspectionService : IInspectionService
    {
        private readonly IInspectionRepository _inspectionRepository;

        public InspectionService(IInspectionRepository inspectionRepository)
        {
            _inspectionRepository = inspectionRepository;
        }

        public async Task<List<Inspection>> GetInspectionsByTechnicianAsync(string userId)
        {
            return await _inspectionRepository.GetInspectionsByTechnicianAsync(userId);
        }

        public async Task<Inspection> GetInspectionByIdAsync(Guid id, string userId)
        {
            return await _inspectionRepository.GetInspectionByIdAsync(id, userId);
        }

        public async Task<Inspection> UpdateInspectionAsync(Guid id, UpdateInspectionRequest request, string userId)
        {
            return await _inspectionRepository.UpdateInspectionAsync(id, request, userId);
        }

        public async Task<Inspection> StartInspectionAsync(Guid id, string userId)
        {
            return await _inspectionRepository.StartInspectionAsync(id, userId);
        }
    }
}