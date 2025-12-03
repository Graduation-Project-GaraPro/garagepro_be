using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;
using Dtos.Quotations;

namespace Services
{
    public interface IInspectionService
    {
        // Basic CRUD operations
        Task<InspectionDto> GetInspectionByIdAsync(Guid inspectionId);
        Task<IEnumerable<InspectionDto>> GetAllInspectionsAsync();
        Task<IEnumerable<InspectionDto>> GetInspectionsByRepairOrderIdAsync(Guid repairOrderId);
        Task<IEnumerable<InspectionDto>> GetInspectionsByTechnicianIdAsync(Guid technicianId);
        Task<InspectionDto> CreateInspectionAsync(CreateInspectionDto createInspectionDto);
        Task<InspectionDto> UpdateInspectionAsync(Guid inspectionId, UpdateInspectionDto updateInspectionDto);
        Task<bool> DeleteInspectionAsync(Guid inspectionId);
        Task<bool> InspectionExistsAsync(Guid inspectionId);

        // Additional operations
        Task<IEnumerable<InspectionDto>> GetPendingInspectionsAsync();
        Task<IEnumerable<InspectionDto>> GetCompletedInspectionsAsync();
        Task<IEnumerable<CompletedInspectionDto>> GetCompletedInspectionsWithDetailsAsync();
        Task<bool> AssignInspectionToTechnicianAsync(Guid inspectionId, Guid technicianId);
        Task<QuotationDto> ConvertInspectionToQuotationAsync(ConvertInspectionToQuotationDto convertDto);
        Task<InspectionDto> CreateManagerInspectionAsync(CreateManagerInspectionDto createManagerInspectionDto);
        Task<IEnumerable<AvailableServiceDto>> GetAvailableServicesForInspectionAsync(Guid repairOrderId);
    }
}