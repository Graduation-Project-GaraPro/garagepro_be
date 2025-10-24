using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BusinessObject.Enums;

namespace Dtos.Quotations
{
    public class CreateInspectionDto
    {
        [Required]
        public Guid RepairOrderId { get; set; }

        public Guid? TechnicianId { get; set; }

        [Required]
        [MaxLength(500)]
        public string CustomerConcern { get; set; }

        // Price for the inspection (to be added to RO total)
        public decimal InspectionPrice { get; set; } = 0;

        // Type of inspection (full or partial)
        public InspectionType InspectionType { get; set; } = InspectionType.Full;
    }

    public class UpdateInspectionDto
    {
        [Required]
        public Guid TechnicianId { get; set; }

        [Required]
        [MaxLength(500)]
        public string CustomerConcern { get; set; }

        [MaxLength(500)]
        public string Finding { get; set; }

        public IssueRating IssueRating { get; set; } = IssueRating.Fair;

        public string Note { get; set; }

        // Price for the inspection (to be added to RO total)
        public decimal InspectionPrice { get; set; } = 0;
        
        public InspectionType InspectionType { get; set; } = InspectionType.Full;
    }

    public class InspectionConcernDto
    {
        public Guid ConcernId { get; set; } = Guid.NewGuid();
        [Required]
        [MaxLength(500)]
        public string Description { get; set; }
    }

    public class UpdateInspectionFindingDto
    {
        public string Finding { get; set; }
        public string Note { get; set; }
        public IssueRating IssueRating { get; set; }
    }

    public class UpdateCustomerConcernDto
    {
        public string CustomerConcern { get; set; }
    }

    public class UpdateInspectionPriceDto
    {
        public decimal InspectionPrice { get; set; }
    }
}