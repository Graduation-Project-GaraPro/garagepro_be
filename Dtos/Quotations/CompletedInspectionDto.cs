using System;
using System.Collections.Generic;
using BusinessObject.Enums;

namespace Dtos.Quotations
{
    public class CompletedInspectionDto
    {
        public Guid InspectionId { get; set; }
        public Guid RepairOrderId { get; set; }
        public Guid? TechnicianId { get; set; }
        public string Status { get; set; }
        public string CustomerConcern { get; set; }
        public string Finding { get; set; }
        public IssueRating IssueRating { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public string TechnicianName { get; set; }
        
        // Services and parts information
        public List<InspectionServiceDto> Services { get; set; } = new List<InspectionServiceDto>();
    }
    
    public class InspectionServiceDto
    {
        public Guid ServiceInspectionId { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public ConditionStatus ConditionStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Parts for this service
        public List<InspectionPartDto> Parts { get; set; } = new List<InspectionPartDto>();
    }
    
    public class InspectionPartDto
    {
        public Guid PartInspectionId { get; set; }
        public Guid PartId { get; set; }
        public string PartName { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}