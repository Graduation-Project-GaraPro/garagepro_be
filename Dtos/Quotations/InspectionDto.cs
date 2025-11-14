using System;
using System.Collections.Generic;
using BusinessObject.Enums;

namespace Dtos.Quotations
{
    public class InspectionDto
    {
        public Guid InspectionId { get; set; }
        public Guid RepairOrderId { get; set; }
        public Guid? TechnicianId { get; set; }
        public string Status { get; set; }
        public string CustomerConcern { get; set; }
        public string Finding { get; set; }
        public IssueRating IssueRating { get; set; }
        public string? Note { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public string TechnicianName { get; set; }
        
        // Services and parts information
        public List<InspectionServiceDto> Services { get; set; } = new List<InspectionServiceDto>();
    }
}