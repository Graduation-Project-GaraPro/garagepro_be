using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
using Dtos.Quotations;
using Repositories;

namespace Services
{
    public class InspectionService : IInspectionService
    {
        private readonly IInspectionRepository _inspectionRepository;
        private readonly IRepairOrderRepository _repairOrderRepository;

        public InspectionService(
            IInspectionRepository inspectionRepository,
            IRepairOrderRepository repairOrderRepository)
        {
            _inspectionRepository = inspectionRepository;
            _repairOrderRepository = repairOrderRepository;
        }

        public async Task<InspectionDto> GetInspectionByIdAsync(Guid inspectionId)
        {
            var inspection = await _inspectionRepository.GetByIdAsync(inspectionId);
            if (inspection == null) return null;

            return MapToDto(inspection);
        }

        public async Task<IEnumerable<InspectionDto>> GetAllInspectionsAsync()
        {
            var inspections = await _inspectionRepository.GetAllAsync();
            return inspections.Select(MapToDto);
        }

        public async Task<IEnumerable<InspectionDto>> GetInspectionsByRepairOrderIdAsync(Guid repairOrderId)
        {
            var inspections = await _inspectionRepository.GetByRepairOrderIdAsync(repairOrderId);
            return inspections.Select(MapToDto);
        }

        public async Task<IEnumerable<InspectionDto>> GetInspectionsByTechnicianIdAsync(Guid technicianId)
        {
            var inspections = await _inspectionRepository.GetByTechnicianIdAsync(technicianId);
            return inspections.Select(MapToDto);
        }

        public async Task<InspectionDto> CreateInspectionAsync(CreateInspectionDto createInspectionDto)
        {
            // Validate that the repair order exists
            var repairOrder = await _repairOrderRepository.GetByIdAsync(createInspectionDto.RepairOrderId);
            if (repairOrder == null)
                throw new ArgumentException("Repair order not found");

            var inspection = new Inspection
            {
                RepairOrderId = createInspectionDto.RepairOrderId,
                TechnicianId = createInspectionDto.TechnicianId,
                CustomerConcern = createInspectionDto.CustomerConcern,
                InspectionPrice = createInspectionDto.InspectionPrice,
                InspectionType = createInspectionDto.InspectionType,
                Status = InspectionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var createdInspection = await _inspectionRepository.CreateAsync(inspection);
            return MapToDto(createdInspection);
        }

        public async Task<InspectionDto> UpdateInspectionAsync(Guid inspectionId, UpdateInspectionDto updateInspectionDto)
        {
            var inspection = await _inspectionRepository.GetByIdAsync(inspectionId);
            if (inspection == null)
                throw new ArgumentException("Inspection not found");

            inspection.TechnicianId = updateInspectionDto.TechnicianId;
            inspection.CustomerConcern = updateInspectionDto.CustomerConcern;
            inspection.Finding = updateInspectionDto.Finding;
            inspection.IssueRating = updateInspectionDto.IssueRating;
            inspection.Note = updateInspectionDto.Note;
            inspection.InspectionPrice = updateInspectionDto.InspectionPrice;
            inspection.InspectionType = updateInspectionDto.InspectionType;
            inspection.UpdatedAt = DateTime.UtcNow;

            var updatedInspection = await _inspectionRepository.UpdateAsync(inspection);
            return MapToDto(updatedInspection);
        }

        public async Task<bool> DeleteInspectionAsync(Guid inspectionId)
        {
            return await _inspectionRepository.DeleteAsync(inspectionId);
        }

        public async Task<bool> InspectionExistsAsync(Guid inspectionId)
        {
            return await _inspectionRepository.ExistsAsync(inspectionId);
        }

        public async Task<IEnumerable<InspectionDto>> GetPendingInspectionsAsync()
        {
            var inspections = await _inspectionRepository.GetPendingInspectionsAsync();
            return inspections.Select(MapToDto);
        }

        public async Task<IEnumerable<InspectionDto>> GetCompletedInspectionsAsync()
        {
            var inspections = await _inspectionRepository.GetCompletedInspectionsAsync();
            return inspections.Select(MapToDto);
        }

        public async Task<bool> AssignInspectionToTechnicianAsync(Guid inspectionId, Guid technicianId)
        {
            return await _inspectionRepository.AssignInspectionToTechnicianAsync(inspectionId, technicianId);
        }

        public async Task<bool> UpdateInspectionFindingAsync(Guid inspectionId, string finding, string note, IssueRating rating)
        {
            return await _inspectionRepository.UpdateInspectionFindingAsync(inspectionId, finding, note, rating);
        }

        public async Task<bool> UpdateCustomerConcernAsync(Guid inspectionId, string concern)
        {
            return await _inspectionRepository.UpdateCustomerConcernAsync(inspectionId, concern);
        }

        public async Task<bool> UpdateInspectionPriceAsync(Guid inspectionId, decimal price)
        {
            return await _inspectionRepository.UpdateInspectionPriceAsync(inspectionId, price);
        }
        
        public async Task<bool> UpdateInspectionTypeAsync(Guid inspectionId, InspectionType type)
        {
            return await _inspectionRepository.UpdateInspectionTypeAsync(inspectionId, type);
        }

        private InspectionDto MapToDto(Inspection inspection)
        {
            return new InspectionDto
            {
                InspectionId = inspection.InspectionId,
                RepairOrderId = inspection.RepairOrderId,
                TechnicianId = inspection.TechnicianId,
                Status = inspection.Status.ToString(),
                CustomerConcern = inspection.CustomerConcern,
                Finding = inspection.Finding,
                IssueRating = inspection.IssueRating,
                Note = inspection.Note,
                InspectionPrice = inspection.InspectionPrice,
                InspectionType = inspection.InspectionType,
                CreatedAt = inspection.CreatedAt,
                UpdatedAt = inspection.UpdatedAt,
                TechnicianName = inspection.Technician?.User?.FullName ?? "Unknown Technician"
            };
        }
    }
}